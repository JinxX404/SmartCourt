using System.Threading.RateLimiting;
using System.Security.Claims;
using System.Globalization;
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
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.Events;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Providers.Jobs;
using SmartCourt.Providers.Payments;
using SmartCourt.Features.Auth.ResendVerification;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RefreshToken;
using SmartCourt.Features.Auth.RegisterClient;
using SmartCourt.Features.Auth.RegisterLawyer;
using SmartCourt.Features.Milestones.Events;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.Integration;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.Dependencies;
using SmartCourt.Features.Contracts.Events;
using SmartCourt.Features.Contracts.Files;
using SmartCourt.Features.Contracts.Integration;

using SmartCourt.Features.Case.Integration;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Features.Chat.Events;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Notifications;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Features.Proposals.Integration;
using SmartCourt.Features.Proposals.Expiration;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Disputes;
using SmartCourt.Features.Payments.FundingVerification;
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
using SmartCourt.Common.Configuration;

namespace SmartCourt;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
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
        services.AddSignalR();

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
            ?? throw new InvalidOperationException("لم يتم العثور على نص الاتصال بقاعدة البيانات (DefaultConnection / LocalConnection).");

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
        services.AddScoped<IProposalExpirationService, ProposalExpirationService>();
        services.AddScoped<
            IContractUserEligibilityService,
            ContractUserEligibilityService>();
        services.AddScoped<
            IContractFileAccessService,
            ContractScopedFileAccessService>();
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
        services.AddScoped<IMilestoneDraftService, MilestoneDraftService>();
        services.AddScoped<IMilestoneChangeRequestService, MilestoneChangeRequestService>();
        services.AddScoped<
            IMilestoneNotificationContextReader,
            MilestoneNotificationContextReader>();
        services.AddScoped<
            IMilestoneAutoAcceptanceService,
            MilestoneAutoAcceptanceService>();
        services.AddScoped<IPaymentEscrowService, PaymentEscrowService>();
        services.AddScoped<IPaymentQueryService, PaymentQueryService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<
            IPaymentNotificationContextReader,
            PaymentNotificationContextReader>();
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
                "يجب إعداد سر التحقق من إشعارات مزود الدفع التجريبي.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(
                    options.ProviderCode),
                "يجب إعداد رمز ثابت لمزود الدفع.")
            .Validate(
                options => options.WebhookMaximumBodySizeBytes
                    is >= 1_024 and <= 1_048_576,
                "يجب أن يكون الحد الأقصى لإشعار مزود الدفع بين 1 كيلوبايت و1 ميجابايت.")
            .Validate(
                options => options.WebhookAllowedIpRanges is not null
                    && options.WebhookAllowedIpRanges.All(
                        range => System.Net.IPNetwork.TryParse(
                            range,
                            out _)),
                "تحتوي قائمة عناوين مزود الدفع المسموح بها على نطاق غير صالح.")
            .Validate(
                options => options.ProcessingSlaMinutes
                    is >= 5 and <= 10_080,
                "يجب أن تكون مهلة معالجة العمليات المالية بين 5 دقائق و7 أيام.")
            .ValidateOnStart();

        // =========================================================================
        // TESTING MODE: Register MockPaymentProvider by default if enabled or as fallback
        // =========================================================================
        if (configuration.GetValue<bool>(
                $"{PaymentProviderOptions.SectionName}:UseMockProvider") || true)
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
        if (!configuration.GetValue<bool>(
                $"{PaymentProviderOptions.SectionName}:UseMockProvider")
            && string.Equals(
                configuration.GetValue<string>(
                    $"{PaymentProviderOptions.SectionName}:ProviderCode"),
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
                    "يجب ضبط BaseUrl و WebhookSecret لمزود Paymob لإثبات ملكية الويب هوك.")
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
        services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailProvider, BackgroundEmailProvider>();

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
        services.Configure<SmartCourt.Providers.ChatModel.DeepSeekChatModelOptions>(configuration.GetSection(SmartCourt.Providers.ChatModel.DeepSeekChatModelOptions.SectionName));
        services.AddHttpClient<IChatModelProvider, SmartCourt.Providers.ChatModel.DeepSeekChatModelProvider>();

        // --- Feature: Document Review ---
        services.AddScoped<SmartCourt.Features.DocumentReview.IDocumentReviewService, SmartCourt.Features.DocumentReview.DocumentReviewService>();

        // --- Feature: Case Review ---
        services.AddScoped<SmartCourt.Features.CaseReview.ICaseReviewService, SmartCourt.Features.CaseReview.CaseReviewService>();

        // --- Feature: Case Analysis ---
        services.AddScoped<SmartCourt.Features.CaseAnalysis.ICaseAnalysisService, SmartCourt.Features.CaseAnalysis.CaseAnalysisService>();

        // --- Feature: Matching ---
        services.AddScoped<SmartCourt.Features.Matching.IMatchingService, SmartCourt.Features.Matching.MatchingService>();

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
