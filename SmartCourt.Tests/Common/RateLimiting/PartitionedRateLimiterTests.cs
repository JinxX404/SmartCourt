using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Disputes;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Payments;
using Xunit;

namespace SmartCourt.Tests.Common.RateLimiting;

public sealed class PartitionedRateLimiterTests
{
    [Theory]
    [MemberData(nameof(ExpectedPolicies))]
    public void SecurityPolicies_MatchRequiredLimits(
        string policyName,
        int ipPermitLimit,
        TimeSpan ipWindow,
        int? userPermitLimit,
        TimeSpan? userWindow)
    {
        var found = SecurityRateLimitPolicies.TryGet(policyName, out var policy);

        Assert.True(found);
        Assert.Equal(ipPermitLimit, policy.Ip.PermitLimit);
        Assert.Equal(ipWindow, policy.Ip.Window);
        Assert.Equal(userPermitLimit, policy.User?.PermitLimit);
        Assert.Equal(userWindow, policy.User?.Window);
    }

    [Fact]
    public void PrivateProfilePolicy_IsolatesAuthenticatedUsers()
    {
        using var serviceProvider = CreateServiceProvider();
        var limiter = GetGlobalLimiter(serviceProvider);
        var firstUser = CreateContext(
            RateLimitPolicyNames.PrivateProfileGet,
            "192.0.2.1",
            "user-1");

        for (var index = 0; index < 120; index++)
        {
            using var lease = limiter.AttemptAcquire(firstUser);
            Assert.True(lease.IsAcquired);
        }

        using var rejectedLease = limiter.AttemptAcquire(firstUser);
        Assert.False(rejectedLease.IsAcquired);

        var secondUser = CreateContext(
            RateLimitPolicyNames.PrivateProfileGet,
            "192.0.2.1",
            "user-2");
        using var isolatedLease = limiter.AttemptAcquire(secondUser);
        Assert.True(isolatedLease.IsAcquired);
    }

    [Fact]
    public void PublicLawyerPolicy_IsolatesClientIpAddresses()
    {
        using var serviceProvider = CreateServiceProvider();
        var limiter = GetGlobalLimiter(serviceProvider);
        var firstIp = CreateContext(
            RateLimitPolicyNames.PublicLawyerGet,
            "192.0.2.10");

        for (var index = 0; index < 120; index++)
        {
            using var lease = limiter.AttemptAcquire(firstIp);
            Assert.True(lease.IsAcquired);
        }

        using var rejectedLease = limiter.AttemptAcquire(firstIp);
        Assert.False(rejectedLease.IsAcquired);

        var secondIp = CreateContext(
            RateLimitPolicyNames.PublicLawyerGet,
            "192.0.2.11");
        using var isolatedLease = limiter.AttemptAcquire(secondIp);
        Assert.True(isolatedLease.IsAcquired);
    }

    [Fact]
    public async Task RejectedRequest_ReturnsGenericJsonApiResponse()
    {
        using var serviceProvider = CreateServiceProvider();
        var applicationBuilder = new ApplicationBuilder(serviceProvider);
        applicationBuilder.Use(async (context, next) =>
        {
            context.SetEndpoint(CreateEndpoint(RateLimitPolicyNames.PublicLawyerGet));
            await next(context);
        });
        applicationBuilder.UseRateLimiter();
        applicationBuilder.Run(_ => Task.CompletedTask);
        var application = applicationBuilder.Build();

        for (var index = 0; index < 120; index++)
        {
            await application(CreateContext("192.0.2.20", serviceProvider));
        }

        var rejectedContext = CreateContext("192.0.2.20", serviceProvider);
        rejectedContext.Response.Body = new MemoryStream();

        await application(rejectedContext);

        rejectedContext.Response.Body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiResponse<string>>(
            rejectedContext.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal(StatusCodes.Status429TooManyRequests, rejectedContext.Response.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(RateLimitResponse.Message, response.Message);
    }

    [Fact]
    public void CriticalSliceEndpoints_AllDeclareSecurityRateLimitPolicies()
    {
        Type[] controllerTypes =
        [
            typeof(ContractsController),
            typeof(MilestonesController),
            typeof(PaymentsController),
            typeof(WalletsController),
            typeof(AdminWalletsController),
            typeof(DisputesController)
        ];

        var actions = controllerTypes
            .SelectMany(type => type.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.DeclaredOnly))
            .Where(method => method
                .GetCustomAttributes(inherit: true)
                .OfType<IActionHttpMethodProvider>()
                .Any())
            .ToArray();

        Assert.Equal(35, actions.Length);
        foreach (var action in actions)
        {
            var attribute = action.GetCustomAttribute<SecurityRateLimitAttribute>();
            Assert.NotNull(attribute);
            Assert.True(SecurityRateLimitPolicies.TryGet(
                attribute.PolicyName,
                out _));
        }
    }

    [Theory]
    [InlineData(typeof(PaymentsController), nameof(PaymentsController.FundAsync),
        RateLimitPolicyNames.FinancialMutation)]
    [InlineData(typeof(PaymentsController), nameof(PaymentsController.HandleWebhookAsync),
        RateLimitPolicyNames.PaymentWebhook)]
    [InlineData(typeof(WalletsController), nameof(WalletsController.WithdrawAsync),
        RateLimitPolicyNames.FinancialMutation)]
    [InlineData(typeof(AdminWalletsController), nameof(AdminWalletsController.AdjustAsync),
        RateLimitPolicyNames.AdminFinancialMutation)]
    [InlineData(typeof(DisputesController), nameof(DisputesController.ResolveAsync),
        RateLimitPolicyNames.AdminFinancialMutation)]
    public void CriticalFinancialEndpoints_UseExpectedPolicy(
        Type controllerType,
        string actionName,
        string expectedPolicy)
    {
        var action = controllerType.GetMethod(actionName);

        Assert.NotNull(action);
        Assert.Equal(
            expectedPolicy,
            action.GetCustomAttribute<SecurityRateLimitAttribute>()?.PolicyName);
    }

    public static TheoryData<string, int, TimeSpan, int?, TimeSpan?> ExpectedPolicies => new()
    {
        {
            RateLimitPolicyNames.PrivateProfileGet,
            300,
            TimeSpan.FromMinutes(1),
            120,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.PrivateProfileUpdate,
            60,
            TimeSpan.FromMinutes(15),
            20,
            TimeSpan.FromMinutes(15)
        },
        {
            RateLimitPolicyNames.PrivateProfileDelete,
            10,
            TimeSpan.FromDays(1),
            3,
            TimeSpan.FromDays(1)
        },
        {
            RateLimitPolicyNames.PublicLawyerGet,
            120,
            TimeSpan.FromMinutes(1),
            null,
            null
        },
        {
            RateLimitPolicyNames.ChangePassword,
            20,
            TimeSpan.FromMinutes(15),
            5,
            TimeSpan.FromMinutes(15)
        },
        {
            RateLimitPolicyNames.ForgotPassword,
            5,
            TimeSpan.FromMinutes(15),
            null,
            null
        },
        {
            RateLimitPolicyNames.ResendVerification,
            5,
            TimeSpan.FromMinutes(15),
            null,
            null
        },
        {
            RateLimitPolicyNames.ResetPassword,
            10,
            TimeSpan.FromMinutes(15),
            null,
            null
        },
        {
            RateLimitPolicyNames.ConfirmEmail,
            20,
            TimeSpan.FromMinutes(15),
            null,
            null
        },
        {
            RateLimitPolicyNames.AuthenticatedQuery,
            300,
            TimeSpan.FromMinutes(1),
            100,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.FinancialQuery,
            120,
            TimeSpan.FromMinutes(1),
            60,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.StandardMutation,
            60,
            TimeSpan.FromMinutes(1),
            20,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.SensitiveMutation,
            30,
            TimeSpan.FromMinutes(1),
            10,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.FinancialMutation,
            15,
            TimeSpan.FromMinutes(1),
            5,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.AdminFinancialMutation,
            10,
            TimeSpan.FromMinutes(1),
            3,
            TimeSpan.FromMinutes(1)
        },
        {
            RateLimitPolicyNames.PaymentWebhook,
            120,
            TimeSpan.FromMinutes(1),
            null,
            null
        }
    };

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApiServices();
        return services.BuildServiceProvider();
    }

    private static System.Threading.RateLimiting.PartitionedRateLimiter<HttpContext> GetGlobalLimiter(
        IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetRequiredService<IOptions<RateLimiterOptions>>()
            .Value
            .GlobalLimiter!;
    }

    private static DefaultHttpContext CreateContext(
        string policyName,
        string ipAddress,
        string? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        context.SetEndpoint(CreateEndpoint(policyName));

        if (userId is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId)],
                "Test"));
        }

        return context;
    }

    private static DefaultHttpContext CreateContext(
        string ipAddress,
        IServiceProvider serviceProvider)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        return context;
    }

    private static Endpoint CreateEndpoint(string policyName)
    {
        return new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new SecurityRateLimitAttribute(policyName)),
            policyName);
    }
}
