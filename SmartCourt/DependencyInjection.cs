using System.Threading.RateLimiting;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;
using SmartCourt.Providers.Jwt;
using SmartCourt.Providers;
using SmartCourt.Common.Extensions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Options;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartCourt.Features.Auth.ConfirmEmail;
using SmartCourt.Features.Auth.ChangePassword;
using SmartCourt.Features.Auth.ForgotPassword;
using SmartCourt.Features.Auth.ResetPassword;
using SmartCourt.Features.Auth.ResendVerification;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RefreshToken;
using SmartCourt.Features.Auth.RegisterClient;
using SmartCourt.Features.Auth.RegisterLawyer;
using SmartCourt.Features.Auth.RevokeRefreshToken;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Entities;
using SmartCourt.Features.UserVerification.DeleteVerificationDocument;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Persistence.DataSeeders;
using SmartCourt.Providers.Email;
using SmartCourt.Providers.FileStorage;
using Twilio.Types;
using static SmartCourt.Interfaces.Providers.IFileStorageService;
using SmartCourt.Providers.VectorStore;
using SmartCourt.Providers.Embedding;
using SmartCourt.Providers.PdfParser;
using SmartCourt.Features.LawIngestion;
using Qdrant.Client;

namespace SmartCourt;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                CreateIpRateLimiter(),
                CreateUserRateLimiter());
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse<string>.Fail(
                        RateLimitResponse.Message,
                        StatusCodes.Status429TooManyRequests),
                    cancellationToken: token);
            };
        });

        services.AddControllers();
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

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        services.AddOptions<MailKitOptions>()
            .Bind(configuration.GetSection("SmtpSettings"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Server)
                && options.Port is > 0 and <= 65535
                && !string.IsNullOrWhiteSpace(options.SenderName)
                && !string.IsNullOrWhiteSpace(options.SenderEmail)
                && !string.IsNullOrWhiteSpace(options.Username)
                && !string.IsNullOrWhiteSpace(options.Password),
                "SMTP settings are incomplete or invalid.")
            .ValidateOnStart();
        services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailProvider, BackgroundEmailProvider>();

        services.AddOptions<AuthEmailOptions>()
            .Bind(configuration.GetSection(AuthEmailOptions.SectionName))
            .Validate(
                options => IsValidPublicBaseUrl(options.PublicBaseUrl, isDevelopment),
                "AuthEmail:PublicBaseUrl must be an absolute public URL outside Development.")
            .ValidateOnStart();

        services.Configure<SmartCourt.Providers.Sms.TwilioOptions>(configuration.GetSection("Twilio"));
        if (isDevelopment)
        {
            services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.MockSmsSender>();
        }
        else
        {
            services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.TwilioSmsSender>();
        }
        services.AddScoped<SmartCourt.Interfaces.Providers.ISmsProvider, SmartCourt.Providers.Sms.BackgroundSmsProvider>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
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

        services.Configure<SupabaseOptions>(configuration.GetSection("Supabase"));
        services.AddScoped<IFileStorageService, SupabaseFileStorageService>();

        services.AddSingleton<Supabase.Client>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SupabaseOptions>>().Value;

            var client = new Supabase.Client(
                options.Url,
                options.ApiKey);

            client.InitializeAsync().GetAwaiter().GetResult();

            return client;
        });

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

        services.AddSingleton<IAccountKeyRateLimiter, AccountKeyRateLimiter>();

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

                    if (user is null
                        || !user.IsAccessEligible()
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
        services.AddScoped<ILoginService, LoginService>();
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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddDataProtection();

        services.AddScoped<SubmitVerificationDocumentsCommand>();
        services.AddScoped<IValidator<SubmitVerificationDocumentsCommand>, SubmitVerificationDocumentsCommandValidator>();

        services.AddScoped<GetUserVerificationDocumentsQuery>();
        services.AddScoped<IValidator<GetUserVerificationDocumentsQuery>, GetUserVerificationDocumentsQueryValidator>();

        services.AddScoped<DeleteVerificationDocumentCommand>();
        services.AddScoped<IValidator<DeleteVerificationDocumentCommand>, DeleteVerificationDocumentCommandValidator>();

        // --- RAG Pipeline: Vector Store ---
        services.Configure<QdrantOptions>(configuration.GetSection(QdrantOptions.SectionName));
        services.AddSingleton<QdrantClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return new QdrantClient(opts.Host, opts.Port, https: opts.UseTls, apiKey: opts.ApiKey);
        });
        services.AddScoped<IVectorStoreProvider, QdrantVectorStoreProvider>();

        // --- RAG Pipeline: Embedding ---
        services.Configure<GeminiEmbeddingOptions>(configuration.GetSection(GeminiEmbeddingOptions.SectionName));
        services.AddHttpClient<IEmbeddingProvider, GeminiEmbeddingProvider>();


        // --- RAG Pipeline: PDF Parser ---
        services.AddScoped<IPdfParserProvider, PdfPigParserProvider>();

        // --- RAG Pipeline: Document Parsing (Composite) ---
        services.AddScoped<IDocumentParsingProvider, SmartCourt.Providers.DocumentParsing.CompositeDocumentParsingProvider>();

        // --- RAG Pipeline: Chat Model ---
        services.Configure<SmartCourt.Providers.ChatModel.GeminiChatModelOptions>(configuration.GetSection(SmartCourt.Providers.ChatModel.GeminiChatModelOptions.SectionName));
        services.AddHttpClient<IChatModelProvider, SmartCourt.Providers.ChatModel.GeminiChatModelProvider>();

        // --- Feature: Document Review ---
        services.AddScoped<SmartCourt.Features.DocumentReview.IDocumentReviewService, SmartCourt.Features.DocumentReview.DocumentReviewService>();

        // --- RAG Pipeline: Law Ingestion Feature ---
        services.Configure<ChunkingOptions>(configuration.GetSection(ChunkingOptions.SectionName));
        services.AddScoped<ILawIngestionService, LawIngestionService>();
        services.AddScoped<LegalDocumentChunker>();

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
