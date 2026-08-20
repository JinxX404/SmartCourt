using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Qdrant.Client;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Extensions;
using SmartCourt.Common.Models;
using SmartCourt.Common.Options;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.ChangePassword;
using SmartCourt.Features.Auth.ConfirmEmail;
using SmartCourt.Features.Auth.ForgotPassword;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RefreshToken;
using SmartCourt.Features.Auth.RegisterClient;
using SmartCourt.Features.Auth.RegisterLawyer;
using SmartCourt.Features.Auth.ResendVerification;
using SmartCourt.Features.Auth.ResetPassword;
using SmartCourt.Features.Auth.RevokeRefreshToken;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Case.Integration;
using SmartCourt.Features.Chat.Events;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.Dependencies;
using SmartCourt.Features.Contracts.Events;
using SmartCourt.Features.Contracts.Files;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Features.Consultations.Bookings;
using SmartCourt.Features.Consultations.Payments;
using SmartCourt.Features.Disputes;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.LawIngestion;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.Events;
using SmartCourt.Features.Milestones.Integration;
using SmartCourt.Features.Notifications;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Articles;
using SmartCourt.Features.Articles.Events;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.Events;
using SmartCourt.Features.Admin.Verifications.Integration;
using SmartCourt.Features.Auth.Events;
using SmartCourt.Features.Auth.Integration;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.Events;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Features.Proposals.Expiration;
using SmartCourt.Features.Proposals.Integration;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Features.UserVerification.DeleteVerificationDocument;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Providers;
using SmartCourt.Providers.Email;
using SmartCourt.Providers.Embedding;
using SmartCourt.Providers.FileStorage;
using SmartCourt.Providers.Jobs;
using SmartCourt.Providers.Jwt;
using SmartCourt.Providers.Payments;
using SmartCourt.Providers.PdfParser;
using SmartCourt.Providers.Reranker;
using SmartCourt.Providers.VectorStore;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace SmartCourt;

public static class DependencyInjection
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
        return services.AddApiServices(
            new ConfigurationBuilder().Build(),
            isDevelopment: false);
    }

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var configuredOrigins = (configuration
                .GetSection("Cors:Origins")
                .Get<string[]>() ?? [])
            .Concat(configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? []);
        var allowedOrigins = configuredOrigins
            .Concat(isDevelopment
                ? ["http://localhost:5173", "http://127.0.0.1:5173",
                    "http://localhost:5188", "http://127.0.0.1:5188"]
                : [])
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy(
                FrontendCorsPolicy,
                policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                });
        });

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                CreateIpRateLimiter(),
                CreateProviderRateLimiter(),
                CreateUserRateLimiter());
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
                }

                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse<string>.Fail(
                        RateLimitResponse.Message,
                        StatusCodes.Status429TooManyRequests),
                    cancellationToken: token);
            };
        });

        services.AddControllers();
        services.AddHealthChecks();
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = isDevelopment;
        });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<SmartCourt.Features.Auth.Login.Validators.LoginRequestValidator>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token in the text input below.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<SmartCourt.Features.Auth.Login.ILoginService, SmartCourt.Features.Auth.Login.LoginService>();
        services.AddSingleton(TimeProvider.System);
        services.AddOptions<OutboxDispatchOptions>()
            .Bind(configuration.GetSection(OutboxDispatchOptions.SectionName))
            .Validate(options => options.BatchSize is >= 1 and <= 1_000,
                "OutboxDispatch:BatchSize must be between 1 and 1000.")
            .Validate(options => options.IdleDelayMilliseconds is >= 100 and <= 60_000,
                "OutboxDispatch:IdleDelayMilliseconds must be between 100 and 60000.")
            .Validate(options => options.ErrorDelayMilliseconds is >= 100 and <= 300_000,
                "OutboxDispatch:ErrorDelayMilliseconds must be between 100 and 300000.")
            .ValidateOnStart();
        services.AddHostedService<OutboxDispatchBackgroundService>();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("LocalConnection")
            ?? throw new InvalidOperationException("Ù„Ù… ÙŠØªÙ… Ø§Ù„Ø¹Ø«ÙˆØ± Ø¹Ù„Ù‰ Ù†Øµ Ø§Ù„Ø§ØªØµØ§Ù„ Ø¨Ù‚Ø§Ø¹Ø¯Ø© Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª (DefaultConnection / LocalConnection).");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString));
        // sqlOptions => sqlOptions.EnableRetryOnFailure()));
        services.AddSingleton<IIdempotencyRequestHasher, CanonicalIdempotencyRequestHasher>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();
        services.AddScoped<INotificationEventMapper, ProposalNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, ContractNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, MilestoneNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, PaymentNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, VerificationNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, AuthNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, ArticleNotificationEventMapper>();
        services.AddScoped<INotificationEventMapper, ConsultationNotificationEventMapper>();
        
        services.AddScoped<IArticleNotificationContextReader, ArticleNotificationContextReader>();
        services.AddScoped<IOutboxEventHandler, NotificationOutboxHandler>();
        services.AddScoped<IOutboxEventHandler, ProposalConversationOutboxHandler>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<
            INotificationRealtimeNotifier,
            SignalRNotificationRealtimeNotifier>();
        services.AddScoped<
            IOutboxEventHandler,
            ContractActivationOutboxHandler>();
        services.AddScoped<
            IOutboxEventHandler,
            MilestoneSchedulingOutboxHandler>();
        services.AddScoped<
            IOutboxEventHandler,
            ContractConversationIntegrationOutboxHandler>();



        services.AddScoped<
            IOutboxEventHandler,
            ContractCaseLifecycleOutboxHandler>();
        services.AddScoped<IChatConversationService, ChatConversationService>();
        services.AddScoped<
            IContractConversationService,
            ContractConversationService>();
        services.AddScoped<IChatRealtimeNotifier, SignalRChatRealtimeNotifier>();
        services.AddScoped<
            IMilestoneSchedulingReconciliationService,
            MilestoneSchedulingReconciliationService>();
        services.AddScoped<
            IContractCreationDependencyGate,
            ContractCreationDependencyGate>();
        services.AddScoped<
            IProposalContractAccessService,
            ProposalContractAccessService>();
        services.AddScoped<
            ICaseContractAccessService,
            CaseContractAccessService>();
        services.AddScoped<
            IContractCaseAssignmentService,
            ContractCaseAssignmentService>();
        services.AddScoped<
            IContractCaseLifecycleService,
            ContractCaseLifecycleService>();
        services.AddScoped<IProposalExpirationService, ProposalExpirationService>();
        services.AddScoped<
            IContractUserEligibilityService,
            ContractUserEligibilityService>();
        services.AddScoped<
            IContractFileAccessService,
            ContractScopedFileAccessService>();
        services.AddScoped<IContractFileService, ContractFileService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IContractActivationEvaluator, ContractService>();
        services.AddScoped<IContractCompletionEvaluator, ContractService>();
        services.AddScoped<
            IContractTerminationRecoveryService,
            ContractService>();
        services.AddScoped<IContractQueryService, ContractQueryService>();
        services.AddScoped<
            IContractNotificationContextReader,
            ContractNotificationContextReader>();
        services.AddScoped<
            IMilestoneFundingVerifier,
            MilestoneFundingVerifier>();
        services.AddScoped<IMilestoneService, MilestoneService>();
        services.AddScoped<
            IMilestoneSubmissionQueryService,
            MilestoneSubmissionQueryService>();
        services.AddScoped<IMilestoneDraftService, MilestoneDraftService>();
        services.AddScoped<IMilestoneChangeRequestService, MilestoneChangeRequestService>();
        services.AddScoped<
            IMilestoneNotificationContextReader,
            MilestoneNotificationContextReader>();
        services.AddScoped<
            IMilestoneAutoAcceptanceService,
            MilestoneAutoAcceptanceService>();
        services.AddScoped<IPaymentEscrowService, PaymentEscrowService>();
        services.AddScoped<IConsultationPaymentService, ConsultationPaymentService>();
        services.AddScoped<IConsultationJobService, ConsultationJobService>();
        services.AddScoped<IPaymentQueryService, PaymentQueryService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<
            IPaymentNotificationContextReader,
            PaymentNotificationContextReader>();
        services.AddScoped<
            IVerificationNotificationContextReader,
            VerificationNotificationContextReader>();
        services.AddScoped<
            IAuthNotificationContextReader,
            AuthNotificationContextReader>();
        services.AddScoped<
            IAdminWalletAdjustmentService,
            AdminWalletAdjustmentService>();
        services.AddScoped<IWalletReconciliationService, WalletReconciliationService>();
        services.AddScoped<IEscrowReleaseService, EscrowReleaseService>();
        services.AddScoped<
            IPaymentProviderStartupValidator,
            PaymentProviderStartupValidator>();
        services.AddScoped<DisputeService>();
        services.AddScoped<IDisputeService>(provider =>
            provider.GetRequiredService<DisputeService>());
        services.AddScoped<IDisputeSettlementRecoveryService>(provider =>
            provider.GetRequiredService<DisputeService>());
        services.AddScoped<
            SmartCourt.Features.Disputes.Penalties.ILawyerPenaltyService,
            SmartCourt.Features.Disputes.Penalties.LawyerPenaltyService>();
        services.AddScoped<
            IContractTerminationSettlementService,
            ContractTerminationSettlementService>();
        services.AddScoped<
            IContractJobOperations,
            PaymentContractJobOperations>();
        services.AddScoped<IContractJobService, ContractJobService>();
        services.AddScoped<IContractJobScheduler, HangfireContractJobScheduler>();


        services.AddOptions<PaymentProviderOptions>()
            .Bind(configuration.GetSection(PaymentProviderOptions.SectionName))
            .Validate(
                options => options.Warning.Contains(
                    "not regulated escrow",
                    StringComparison.OrdinalIgnoreCase),
                "The mock payment provider warning must state that it is not regulated escrow.")
            .Validate(
                options => !options.UseMockProvider
                    || !string.IsNullOrWhiteSpace(
                        options.WebhookSecret),
                "ÙŠØ¬Ø¨ Ø¥Ø¹Ø¯Ø§Ø¯ Ø³Ø± Ø§Ù„ØªØ­Ù‚Ù‚ Ù…Ù† Ø¥Ø´Ø¹Ø§Ø±Ø§Øª Ù…Ø²ÙˆØ¯ Ø§Ù„Ø¯ÙØ¹ Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠ.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(
                    options.ProviderCode),
                "ÙŠØ¬Ø¨ Ø¥Ø¹Ø¯Ø§Ø¯ Ø±Ù…Ø² Ø«Ø§Ø¨Øª Ù„Ù…Ø²ÙˆØ¯ Ø§Ù„Ø¯ÙØ¹.")
            .Validate(
                options => options.WebhookMaximumBodySizeBytes
                    is >= 1_024 and <= 1_048_576,
                "ÙŠØ¬Ø¨ Ø£Ù† ÙŠÙƒÙˆÙ† Ø§Ù„Ø­Ø¯ Ø§Ù„Ø£Ù‚ØµÙ‰ Ù„Ø¥Ø´Ø¹Ø§Ø± Ù…Ø²ÙˆØ¯ Ø§Ù„Ø¯ÙØ¹ Ø¨ÙŠÙ† 1 ÙƒÙŠÙ„ÙˆØ¨Ø§ÙŠØª Ùˆ1 Ù…ÙŠØ¬Ø§Ø¨Ø§ÙŠØª.")
            .Validate(
                options => options.WebhookAllowedIpRanges is not null
                    && options.WebhookAllowedIpRanges.All(
                        range => System.Net.IPNetwork.TryParse(
                            range,
                            out _)),
                "ØªØ­ØªÙˆÙŠ Ù‚Ø§Ø¦Ù…Ø© Ø¹Ù†Ø§ÙˆÙŠÙ† Ù…Ø²ÙˆØ¯ Ø§Ù„Ø¯ÙØ¹ Ø§Ù„Ù…Ø³Ù…ÙˆØ­ Ø¨Ù‡Ø§ Ø¹Ù„Ù‰ Ù†Ø·Ø§Ù‚ ØºÙŠØ± ØµØ§Ù„Ø­.")
            .Validate(
                options => options.ProcessingSlaMinutes
                    is >= 5 and <= 10_080,
                "ÙŠØ¬Ø¨ Ø£Ù† ØªÙƒÙˆÙ† Ù…Ù‡Ù„Ø© Ù…Ø¹Ø§Ù„Ø¬Ø© Ø§Ù„Ø¹Ù…Ù„ÙŠØ§Øª Ø§Ù„Ù…Ø§Ù„ÙŠØ© Ø¨ÙŠÙ† 5 Ø¯Ù‚Ø§Ø¦Ù‚ Ùˆ7 Ø£ÙŠØ§Ù….")
            .ValidateOnStart();

        var useMockPaymentProvider = configuration.GetValue<bool?>(
            $"{PaymentProviderOptions.SectionName}:UseMockProvider")
            ?? true;
        var paymentProviderCode = configuration.GetValue<string>(
            $"{PaymentProviderOptions.SectionName}:ProviderCode");

        if (useMockPaymentProvider)
        {
            services.AddScoped<MockPaymentProvider>();
            services.AddScoped<IPaymentProvider>(
                provider => provider
                    .GetRequiredService<MockPaymentProvider>());
            services.AddScoped<IPaymentReconciliationProvider>(
                provider => provider
                    .GetRequiredService<MockPaymentProvider>());
        }

        // =========================================================================
        // PAYMOB MARKETPLACE: explicit opt-in by setting
        // "PaymentProvider:ProviderCode" to "PaymobMarketPlace".
        // The mock stays the default; choosing Paymob overrides the interface resolution.
        // =========================================================================
        else if (string.Equals(
                paymentProviderCode,
                SmartCourt.Providers.Payments.Stripe.StripeOptions.ProviderCode,
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddOptions<SmartCourt.Providers.Payments.Stripe.StripeOptions>()
                .Bind(configuration.GetSection(
                    $"{PaymentProviderOptions.SectionName}:{SmartCourt.Providers.Payments.Stripe.StripeOptions.SectionName}"))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.SecretKey)
                        && options.SecretKey.StartsWith("sk_test_", StringComparison.Ordinal),
                    "Stripe:SecretKey must be a Stripe test secret key (sk_test_...).")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.PublishableKey)
                        && options.PublishableKey.StartsWith("pk_test_", StringComparison.Ordinal),
                    "Stripe:PublishableKey must be a Stripe test publishable key (pk_test_...).")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.PlatformWebhookSecret)
                        && !string.IsNullOrWhiteSpace(options.ConnectWebhookSecret),
                    "Both Stripe platform and Connect webhook signing secrets are required.")
                .Validate(
                    options => options.SandboxOnly
                        && options.WebhookToleranceSeconds is >= 60 and <= 900
                        && options.MaxNetworkRetries is >= 0 and <= 5
                        && Uri.IsWellFormedUriString(options.ConnectReturnUrl, UriKind.Absolute)
                        && Uri.IsWellFormedUriString(options.ConnectRefreshUrl, UriKind.Absolute),
                    "Stripe Connect sandbox, webhook tolerance, retries, and onboarding URLs are invalid.")
                .ValidateOnStart();

            services.AddSingleton<global::Stripe.StripeClient>(serviceProvider =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<SmartCourt.Providers.Payments.Stripe.StripeOptions>>()
                    .Value;
                var httpClient =
                    global::Stripe.SystemNetHttpClient
                        .BuildDefaultSystemNetHttpClient();
                var stripeHttpClient = new global::Stripe.SystemNetHttpClient(
                    httpClient,
                    options.MaxNetworkRetries,
                    appInfo: null,
                    enableTelemetry: true);
                return new global::Stripe.StripeClient(
                    new global::Stripe.StripeClientOptions
                    {
                        ApiKey = options.SecretKey,
                        HttpClient = stripeHttpClient
                    });
            });
            services.AddScoped<SmartCourt.Providers.Payments.Stripe.StripePaymentProvider>();
            services.AddScoped<SmartCourt.Providers.Payments.Stripe.StripeWebhookVerifier>();
            services.AddScoped<PaymentProviderWebhookService>();
            services.AddScoped<ILawyerPayoutAccountService, LawyerPayoutAccountService>();
            services.AddScoped<IClientPaymentMethodService, ClientPaymentMethodService>();
            services.AddScoped<IPaymentProvider>(serviceProvider => serviceProvider
                .GetRequiredService<SmartCourt.Providers.Payments.Stripe.StripePaymentProvider>());
            services.AddScoped<IPaymentReconciliationProvider>(serviceProvider => serviceProvider
                .GetRequiredService<SmartCourt.Providers.Payments.Stripe.StripePaymentProvider>());
            services.AddScoped<ILawyerPayoutAccountProvider>(serviceProvider => serviceProvider
                .GetRequiredService<SmartCourt.Providers.Payments.Stripe.StripePaymentProvider>());
            services.AddScoped<IClientPaymentMethodProvider>(serviceProvider => serviceProvider
                .GetRequiredService<SmartCourt.Providers.Payments.Stripe.StripePaymentProvider>());
            services.AddScoped<IPaymentBrowserConfigurationProvider>(serviceProvider => serviceProvider
                .GetRequiredService<SmartCourt.Providers.Payments.Stripe.StripePaymentProvider>());
            services.AddScoped<IPaymentProviderWebhookVerifier>(serviceProvider => serviceProvider
                .GetRequiredService<SmartCourt.Providers.Payments.Stripe.StripeWebhookVerifier>());
        }
        else if (!useMockPaymentProvider
            && string.Equals(
                paymentProviderCode,
                PaymobOptions.ProviderCode,
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddOptions<PaymobOptions>()
                .Bind(configuration.GetSection(
                    $"{PaymentProviderOptions.SectionName}:{PaymobOptions.SectionName}"))
                .Validate(
                    options => Uri.IsWellFormedUriString(
                        options.BaseUrl,
                        UriKind.Absolute)
                        && !string.IsNullOrWhiteSpace(options.WebhookSecret),
                    "ÙŠØ¬Ø¨ Ø¶Ø¨Ø· BaseUrl Ùˆ WebhookSecret Ù„Ù…Ø²ÙˆØ¯ Paymob Ù„Ø¥Ø«Ø¨Ø§Øª Ù…Ù„ÙƒÙŠØ© Ø§Ù„ÙˆÙŠØ¨ Ù‡ÙˆÙƒ.")
                .ValidateOnStart();

            services.AddHttpClient<PaymobPaymentProvider>(
                (sp, client) =>
                {
                    var options = sp
                        .GetRequiredService<IOptions<PaymobOptions>>()
                        .Value;
                    client.Timeout = TimeSpan.FromSeconds(
                        options.TimeoutSeconds);
                });

            services.AddScoped<IPaymentProvider>(
                sp => sp.GetRequiredService<PaymobPaymentProvider>());
            services.AddScoped<IPaymentReconciliationProvider>(
                sp => sp.GetRequiredService<PaymobPaymentProvider>());
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported payment provider '{paymentProviderCode ?? "<null>"}'.");
        }

        services.AddOptions<MailKitOptions>()
            .Bind(configuration.GetSection("SmtpSettings"))
            //.Validate(options =>
            //    !string.IsNullOrWhiteSpace(options.Server)
            //    && options.Port is > 0 and <= 65535
            //    && !string.IsNullOrWhiteSpace(options.SenderName)
            //    && !string.IsNullOrWhiteSpace(options.SenderEmail)
            //    && !string.IsNullOrWhiteSpace(options.Username)
            //    && !string.IsNullOrWhiteSpace(options.Password),
            //    "SMTP settings are incomplete or invalid.")
            .ValidateOnStart();
        if (string.Equals(
                configuration["Email:Provider"],
                "Mock",
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ISmtpEmailSender, MockSmtpEmailSender>();
        }
        else
        {
            services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
        }
        services.AddScoped<IEmailProvider, DirectEmailProvider>();

        services.AddOptions<AuthEmailOptions>()
            .Bind(configuration.GetSection(AuthEmailOptions.SectionName))
            //.Validate(
            //    options => IsValidPublicBaseUrl(options.PublicBaseUrl, isDevelopment),
            //    "AuthEmail:PublicBaseUrl must be an absolute public HTTPS URL outside Development.")
            .ValidateOnStart();

        services.Configure<SmartCourt.Providers.Sms.TwilioOptions>(configuration.GetSection("Twilio"));
        // =========================================================================
        // TESTING MODE: Use MockSmsSender in all environments (including Production).
        // To re-enable Twilio SMS in Production, uncomment the block below:
        // =========================================================================
        /*
        if (isDevelopment)
        {
            services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.MockSmsSender>();
        }
        else
        {
            services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.TwilioSmsSender>();
        }
        */
        services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.MockSmsSender>();
        services.AddScoped<SmartCourt.Interfaces.Providers.ISmsProvider, SmartCourt.Providers.Sms.BackgroundSmsProvider>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                PrepareSchemaIfNecessary = true
            }));
        services.AddHangfireServer();
        services.AddScoped<SmartCourt.Interfaces.Providers.IBackgroundJobProvider, SmartCourt.Providers.Jobs.HangfireJobProvider>();
        services.AddSingleton<
            SmartCourt.Interfaces.Providers.IRecurringBackgroundJobProvider,
            SmartCourt.Providers.Jobs.HangfireRecurringBackgroundJobProvider>();
        services.AddScoped<
            IContractRecurringJobRegistrar,
            ContractRecurringJobRegistrar>();
        services.AddScoped<
            IProposalRecurringJobRegistrar,
            ProposalRecurringJobRegistrar>();

        services.Configure<FileStorageOptions>(
            configuration.GetSection(FileStorageOptions.SectionName));

        var fileStorageProvider = configuration.GetValue<string>(
            $"{FileStorageOptions.SectionName}:Provider");
        if (string.Equals(
                fileStorageProvider,
                "Local",
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<
                IFileStorageService,
                LocalFileStorageService>();
        }
        else
        {
            services.Configure<SupabaseOptions>(
                configuration.GetSection("Supabase"));
            services.AddScoped<
                IFileStorageService,
                SupabaseFileStorageService>();

            services.AddSingleton<Supabase.Client>(sp =>
            {
                var options = sp
                    .GetRequiredService<IOptions<SupabaseOptions>>()
                    .Value;

                var client = new Supabase.Client(
                    options.Url,
                    options.ApiKey);

                client.InitializeAsync().GetAwaiter().GetResult();

                return client;
            });
        }

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IAccountKeyRateLimiter, AccountKeyRateLimiter>();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(1);
        });

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var key = Encoding.UTF8.GetBytes(jwtOptions.Secret ?? string.Empty);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // 1. SignalR passes token via query string
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrWhiteSpace(accessToken)
                        && (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/notifications")))
                    {
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }

                    // 2. Read from HttpOnly cookie (primary auth mechanism)
                    var cookieToken = context.Request.Cookies["accessToken"];
                    if (!string.IsNullOrWhiteSpace(cookieToken))
                    {
                        context.Token = cookieToken;
                    }

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userId, out var parsedUserId))
                    {
                        context.Fail("Invalid access token.");
                        return;
                    }

                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<ApplicationUser>>();
                    var user = await userManager.FindByIdAsync(parsedUserId.ToString());

                    var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                    bool isProfileCompletion = path.EndsWith("/profile/complete", StringComparison.OrdinalIgnoreCase);
                    bool isEligible = user != null && (user.IsAccessEligible() || (user.EmailConfirmed && user.Status == SmartCourt.Features.Auth.Enums.UserStatus.Unverified && isProfileCompletion));

                    if (user is null
                        || !isEligible
                        || context.Principal is null
                        || !user.HasValidSecurityStamp(context.Principal))
                    {
                        context.Fail("Invalid access token.");
                    }
                }
            };
        });

        services.AddAuthorization();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IAuthHelperService, AuthHelperService>();
        services.AddScoped<SmartCourt.Features.ChatAgent.IQuotaService, SmartCourt.Features.ChatAgent.QuotaService>();
        services.AddScoped<SmartCourt.Features.Admin.Quotas.IAdminQuotaService, SmartCourt.Features.Admin.Quotas.AdminQuotaService>();
        services.AddScoped<SmartCourt.Features.ChatAgent.Monetization.ITokenBundlePurchaseService, SmartCourt.Features.ChatAgent.Monetization.TokenBundlePurchaseService>();
        services.AddScoped<SmartCourt.Features.ChatAgent.Monetization.ITokenBundleFulfillmentService, SmartCourt.Features.ChatAgent.Monetization.TokenBundleFulfillmentService>();

        services.AddScoped<IConfirmEmailService, ConfirmEmailService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRegisterClientService, RegisterClientService>();
        services.AddScoped<IRegisterLawyerService, RegisterLawyerService>();
        services.AddScoped<IRevokeRefreshTokenService, RevokeRefreshTokenService>();
        services.AddScoped<IChangePasswordService, ChangePasswordService>();
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<IResendVerificationService, ResendVerificationService>();
        services.AddScoped<SmartCourt.Features.Users.Lawyers.ILawyerService, SmartCourt.Features.Users.Lawyers.LawyerService>();
        services.AddScoped<SmartCourt.Features.Users.Clients.IClientService, SmartCourt.Features.Users.Clients.ClientService>();
        
        services.AddScoped<SmartCourt.Features.LawyerSubscription.ILawyerQuotaService, SmartCourt.Features.LawyerSubscription.LawyerQuotaService>();
        services.AddScoped<SmartCourt.Features.LawyerSubscription.ILawyerSubscriptionPaymentService, SmartCourt.Features.LawyerSubscription.LawyerSubscriptionPaymentService>();
        services.AddScoped<SmartCourt.Features.Admin.LawyerSubscriptions.IAdminLawyerSubscriptionService, SmartCourt.Features.Admin.LawyerSubscriptions.AdminLawyerSubscriptionService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddDataProtection();

        services.AddScoped<SubmitVerificationDocumentsCommand>();
        services.AddScoped<IValidator<SubmitVerificationDocumentsCommand>, SubmitVerificationDocumentsCommandValidator>();

        services.AddScoped<GetUserVerificationDocumentsQuery>();
        services.AddScoped<IValidator<GetUserVerificationDocumentsQuery>, GetUserVerificationDocumentsQueryValidator>();

        services.AddScoped<DeleteVerificationDocumentCommand>();
        services.AddScoped<IValidator<DeleteVerificationDocumentCommand>, DeleteVerificationDocumentCommandValidator>();

        // --- RAG Pipeline: Vector Store ---
        services.AddOptions<QdrantOptions>()
            .Bind(configuration.GetSection(QdrantOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.Host), "Qdrant:Host is required.")
            .Validate(x => x.Port > 0, "Qdrant:Port must be positive.")
            .ValidateOnStart();
        services.AddOptions<RagOptions>()
            .Bind(configuration.GetSection(RagOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.LegalCollectionName), "Rag:LegalCollectionName is required.")
            .Validate(x => x.EmbeddingBatchSize is > 0 and <= 128, "Rag:EmbeddingBatchSize must be between 1 and 128.")
            .Validate(x => x.CandidateCount is > 0 and <= 100, "Rag:CandidateCount must be between 1 and 100.")
            .Validate(x => x.RerankedCount is > 0 and <= 20, "Rag:RerankedCount must be between 1 and 20.")
            .Validate(x => x.MinimumSimilarityScore is >= -1 and <= 1, "Rag:MinimumSimilarityScore must be between -1 and 1.")
            .ValidateOnStart();
        services.AddOptions<SmartCourt.Common.Configuration.QuotaOptions>()
            .Bind(configuration.GetSection(SmartCourt.Common.Configuration.QuotaOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SmartCourt.Common.Configuration.LawyerPlanOptions>()
            .Bind(configuration.GetSection(SmartCourt.Common.Configuration.LawyerPlanOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.Configure<List<SmartCourt.Common.Configuration.TokenBundleOptions>>(configuration.GetSection("TokenBundles"));
        services.Configure<List<SmartCourt.Common.Configuration.LawyerTokenBundleOptions>>(configuration.GetSection("LawyerTokenBundles"));

        services.AddSingleton<QdrantClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return new QdrantClient(opts.Host, opts.Port, https: opts.UseTls, apiKey: opts.ApiKey);
        });
        services.AddScoped<IVectorStoreProvider, QdrantVectorStoreProvider>();

        // --- RAG Pipeline: Embedding ---
        services.AddOptions<AlibabaEmbeddingOptions>()
            .Bind(configuration.GetSection(AlibabaEmbeddingOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "AlibabaEmbedding:ApiKey is required.")
            .Validate(x => Uri.TryCreate(x.BaseUrl, UriKind.Absolute, out _), "AlibabaEmbedding:BaseUrl must be absolute.")
            .Validate(x => x.Dimensions > 0, "AlibabaEmbedding:Dimensions must be positive.")
            .ValidateOnStart();
        services.AddHttpClient<IEmbeddingProvider, AlibabaEmbeddingProvider>();

        // --- RAG Pipeline: Reranker ---
        services.AddOptions<AlibabaRerankerOptions>()
            .Bind(configuration.GetSection(AlibabaRerankerOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "AlibabaReranker:ApiKey is required.")
            .Validate(x => Uri.TryCreate(x.BaseUrl, UriKind.Absolute, out _), "AlibabaReranker:BaseUrl must be absolute.")
            .ValidateOnStart();
        services.AddHttpClient<IRerankerProvider, AlibabaRerankerProvider>();


        // --- RAG Pipeline: PDF Parser ---
        services.AddScoped<IPdfParserProvider, PdfPigParserProvider>();

        // --- RAG Pipeline: Document Parsing (Composite) ---
        services.AddScoped<IDocumentParsingProvider, SmartCourt.Providers.DocumentParsing.CompositeDocumentParsingProvider>();

        // --- RAG Pipeline: Chat Model ---
        // services.Configure<SmartCourt.Providers.ChatModel.DeepSeekChatModelOptions>(configuration.GetSection(SmartCourt.Providers.ChatModel.DeepSeekChatModelOptions.SectionName));
        // services.AddHttpClient<IChatModelProvider, SmartCourt.Providers.ChatModel.DeepSeekChatModelProvider>();
        services.AddOptions<SmartCourt.Providers.ChatModel.AlibabaChatModelOptions>()
            .Bind(configuration.GetSection(SmartCourt.Providers.ChatModel.AlibabaChatModelOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "AlibabaChatModel:ApiKey is required.")
            .Validate(x => Uri.TryCreate(x.BaseUrl, UriKind.Absolute, out _), "AlibabaChatModel:BaseUrl must be absolute.")
            .Validate(x => x.MaxTokens > 0, "AlibabaChatModel:MaxTokens must be positive.")
            .ValidateOnStart();
        services.AddHttpClient<IChatModelProvider, SmartCourt.Providers.ChatModel.AlibabaChatModelProvider>();
        // --- Feature: Add Case Document ---
        services.AddScoped<SmartCourt.Features.Case.AddCaseDocument.IAddCaseDocumentService, SmartCourt.Features.Case.AddCaseDocument.AddCaseDocumentService>();

        // --- Feature: Document Review ---
        services.AddScoped<SmartCourt.Features.DocumentReview.IDocumentReviewService, SmartCourt.Features.DocumentReview.DocumentReviewService>();

        // --- Feature: Case Review ---
        services.AddScoped<SmartCourt.Features.CaseReview.ICaseReviewService, SmartCourt.Features.CaseReview.CaseReviewService>();

        // --- Feature: Case Analysis ---
        services.AddScoped<SmartCourt.Features.CaseAnalysis.ICaseAnalysisService, SmartCourt.Features.CaseAnalysis.CaseAnalysisService>();

        // --- Feature: Chat Agent ---
        services.AddScoped<SmartCourt.Features.ChatAgent.IChatAgentService, SmartCourt.Features.ChatAgent.ChatAgentService>();
        services.AddScoped<SmartCourt.Features.ChatAgent.IQuotaService, SmartCourt.Features.ChatAgent.QuotaService>();
        services.AddScoped<SmartCourt.Features.ChatAgent.ICostCalculatorService, SmartCourt.Features.ChatAgent.CostCalculatorService>();

        // --- Feature: Matching ---
        services.AddScoped<SmartCourt.Features.Matching.IMatchingService, SmartCourt.Features.Matching.MatchingService>();

        // --- Feature: Articles ---
        services.AddScoped<SmartCourt.Features.Articles.IArticleService, SmartCourt.Features.Articles.ArticleService>();

        // --- RAG Pipeline: Law Ingestion Feature ---
        services.AddOptions<ChunkingOptions>()
            .Bind(configuration.GetSection(ChunkingOptions.SectionName))
            .Validate(x => x.MaxChunkTokens > 0 && x.OverlapTokens >= 0 && x.OverlapTokens < x.MaxChunkTokens,
                "Chunking overlap must be non-negative and less than the maximum chunk size.")
            .Validate(x => x.MinChunkTokens > 0 && x.MinChunkTokens <= x.MaxChunkTokens,
                "Chunking minimum size must be positive and not exceed the maximum chunk size.")
            .ValidateOnStart();
        services.AddScoped<ILawIngestionService, LawIngestionService>();
        services.AddScoped<LegalDocumentChunker>();

        // --- Feature: Ratings ---
        services.AddScoped<SmartCourt.Features.Ratings.IRatingService, SmartCourt.Features.Ratings.RatingService>();

        // --- Feature: Lawyer Dashboard ---
        services.AddScoped<SmartCourt.Features.Users.Lawyers.Dashboard.ILawyerDashboardService, SmartCourt.Features.Users.Lawyers.Dashboard.LawyerDashboardService>();

        return services;
    }

    private static PartitionedRateLimiter<HttpContext> CreateIpRateLimiter()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var attribute = context.GetEndpoint()?.Metadata.GetMetadata<SecurityRateLimitAttribute>();
            if (attribute is null
                || !SecurityRateLimitPolicies.TryGet(attribute.PolicyName, out var policy))
            {
                return RateLimitPartition.GetNoLimiter("ip:none");
            }

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return CreateFixedWindowPartition(
                $"{attribute.PolicyName}:ip:{clientIp}",
                policy.Ip);
        });
    }

    private static PartitionedRateLimiter<HttpContext> CreateUserRateLimiter()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var attribute = context.GetEndpoint()?.Metadata.GetMetadata<SecurityRateLimitAttribute>();
            if (attribute is null
                || !SecurityRateLimitPolicies.TryGet(attribute.PolicyName, out var policy)
                || policy.User is null)
            {
                return RateLimitPartition.GetNoLimiter("user:none");
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (context.User.Identity?.IsAuthenticated != true
                || string.IsNullOrWhiteSpace(userId))
            {
                return RateLimitPartition.GetNoLimiter("user:anonymous");
            }

            return CreateFixedWindowPartition(
                $"{attribute.PolicyName}:user:{userId}",
                policy.User);
        });
    }

    private static PartitionedRateLimiter<HttpContext>
        CreateProviderRateLimiter()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var attribute = context.GetEndpoint()?.Metadata
                .GetMetadata<SecurityRateLimitAttribute>();
            if (attribute is null
                || !SecurityRateLimitPolicies.TryGet(
                    attribute.PolicyName,
                    out var policy)
                || policy.Provider is null)
            {
                return RateLimitPartition.GetNoLimiter("provider:none");
            }

            var providerCode = context.RequestServices
                .GetService<IOptions<PaymentProviderOptions>>()?
                .Value.ProviderCode;
            var normalizedProviderCode =
                string.IsNullOrWhiteSpace(providerCode)
                    ? "unconfigured"
                    : providerCode.Trim().ToUpperInvariant();
            return CreateFixedWindowPartition(
                $"{attribute.PolicyName}:provider:{normalizedProviderCode}",
                policy.Provider);
        });
    }

    private static RateLimitPartition<string> CreateFixedWindowPartition(
        string partitionKey,
        RateLimitBucket bucket)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = bucket.PermitLimit,
                Window = bucket.Window,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static bool IsValidPublicBaseUrl(string value, bool isDevelopment)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || string.IsNullOrWhiteSpace(uri.Host)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Temporarily allowing HTTP in production as requested by the user
        return isDevelopment || !uri.IsLoopback;
    }
}

