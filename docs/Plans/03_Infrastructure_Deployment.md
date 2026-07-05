# Smart Court — Infrastructure & Deployment

> **Version:** 1.0 | **Date:** 2026-07-03
> **Hosting:** On-premises (initial) | **OS:** Windows Server / Linux
> **Database:** SQL Server | **Cache:** In-Memory (IMemoryCache)

---

## 1. Solution Structure

```
SmartCourt/
├── src/
│   ├── SmartCourt.Core/              # Entities, Interfaces, Enums, DTOs shared
│   │   ├── Entities/
│   │   │   ├── Identity/             # AspNetUsers, ClientProfile, LawyerProfile...
│   │   │   ├── Cases/                # LegalCase, AIAnalysis, LawyerMatch...
│   │   │   ├── Proposals/            # Proposal, Conversation, Message...
│   │   │   ├── Contracts/            # Contract, Milestone, Payment...
│   │   │   ├── Reviews/              # Review, Dispute...
│   │   │   ├── AI/                   # AIConversation, AIMessage...
│   │   │   ├── Articles/             # LegalArticle, LegalArticleCategory...
│   │   │   ├── Notifications/        # Notification, UserNotification...
│   │   │   └── Audit/               # StatusChangeLog, EscrowTransaction...
│   │   ├── Enums/
│   │   │   ├── CaseStatus.cs
│   │   │   ├── ContractStatus.cs
│   │   │   ├── MilestoneStatus.cs
│   │   │   ├── ProposalStatus.cs
│   │   │   ├── PaymentTransactionStatus.cs
│   │   │   ├── DisputeStatus.cs
│   │   │   ├── ArticleStatus.cs
│   │   │   ├── VerificationStatus.cs
│   │   │   ├── NotificationType.cs
│   │   │   └── MessageType.cs
│   │   ├── Providers/
│   │   │   ├── ILlmProvider.cs
│   │   │   ├── IVectorStoreProvider.cs
│   │   │   ├── IPaymentProvider.cs
│   │   │   ├── IFileStorageProvider.cs
│   │   │   ├── IEmailProvider.cs
│   │   │   └── ISmsProvider.cs
│   │   └── Common/
│   │       ├── ApiResponse.cs
│   │       ├── PagedRequest.cs
│   │       └── PagedResponse.cs
│   │
│   ├── SmartCourt.Infrastructure/    # DbContext, Providers, Configs
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/       # EF Core Fluent API (1 file per entity)
│   │   │   ├── Migrations/
│   │   │   └── Seeders/
│   │   │       └── LegalCategorySeeder.cs
│   │   └── Providers/
│   │       ├── OpenAiProvider.cs
│   │       ├── QdrantProvider.cs
│   │       ├── LocalFileStorageProvider.cs
│   │       ├── StubPaymentProvider.cs
│   │       ├── SmtpEmailProvider.cs
│   │       └── ConsoleSmsProvider.cs
│   │
│   └── SmartCourt.API/              # Feature Slices + Startup
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── Prompts/                  # AI prompt templates (.txt files)
│       ├── Hubs/
│       │   └── ChatHub.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       └── Features/
│           ├── Auth/
│           │   ├── AuthController.cs
│           │   ├── AuthService.cs
│           │   ├── DTOs/
│           │   └── Validators/
│           ├── Users/
│           ├── Cases/
│           ├── AIAnalysis/
│           ├── LawyerMatching/
│           ├── Marketplace/
│           ├── Proposals/
│           ├── Chat/
│           ├── Contracts/
│           ├── Payments/
│           ├── Articles/
│           ├── Reviews/
│           ├── Disputes/
│           ├── Notifications/
│           ├── AIAssistant/
│           ├── FileUpload/
│           ├── LawyerVerification/
│           └── Admin/
│
├── tests/
│   ├── SmartCourt.UnitTests/
│   └── SmartCourt.IntegrationTests/
│
├── docker-compose.yml
├── docker-compose.override.yml
├── Dockerfile
├── .github/workflows/ci.yml
├── SmartCourt.sln
└── README.md
```

---

## 2. Docker Compose (Local Development)

```yaml
# docker-compose.yml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "SmartCourt_Dev_2026!"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  # Qdrant Vector Database (for RAG)
  qdrant:
    image: qdrant/qdrant:latest
    ports:
      - "6333:6333"    # REST API
      - "6334:6334"    # gRPC
    volumes:
      - qdrant_data:/qdrant/storage

  # Seq (Structured Logging UI) — Free for single user
  seq:
    image: datalust/seq:latest
    environment:
      ACCEPT_EULA: "Y"
    ports:
      - "5341:5341"    # Ingestion
      - "8081:80"      # UI

  # MailHog (Email testing — catches all SMTP)
  mailhog:
    image: mailhog/mailhog:latest
    ports:
      - "1025:1025"    # SMTP
      - "8025:8025"    # Web UI

volumes:
  sqlserver_data:
  qdrant_data:
```

---

## 3. Program.cs Pipeline

```csharp
var builder = WebApplication.CreateBuilder(args);

// === Services ===

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => {
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        // SignalR token from query string
        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("SmartCourtPolicy", policy => {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("Default", opt => {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    options.AddFixedWindowLimiter("Auth", opt => {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(5);
    });
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// SignalR
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { /* JWT config */ });
});

// Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341"));

// === Provider Registration (DI) ===
builder.Services.AddScoped<ILlmProvider, OpenAiProvider>();
builder.Services.AddScoped<IVectorStoreProvider, QdrantProvider>();
builder.Services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
builder.Services.AddScoped<IPaymentProvider, StubPaymentProvider>();
builder.Services.AddScoped<IEmailProvider, SmtpEmailProvider>();
builder.Services.AddScoped<ISmsProvider, ConsoleSmsProvider>();

// === Feature Service Registration ===
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
// ... all 18 feature services

// Memory Cache
builder.Services.AddMemoryCache();

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddUrlGroup(new Uri("http://localhost:6333"), name: "qdrant");

// === App ===
var app = builder.Build();

// Middleware Pipeline (order matters!)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("SmartCourtPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHealthChecks("/health");

// Auto-migrate in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
```

---

## 4. Environment Configuration

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SmartCourt_Dev;User Id=sa;Password=SmartCourt_Dev_2026!;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "dev-secret-key-min-32-characters-long!!",
    "Issuer": "SmartCourt.API",
    "Audience": "SmartCourt.Client",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Cors": {
    "Origins": ["http://localhost:3000", "http://localhost:5173"]
  },
  "FileStorage": {
    "Provider": "Local",
    "BasePath": "./uploads"
  },
  "Email": {
    "Provider": "MailHog",
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "FromEmail": "noreply@smartcourt.dev",
    "FromName": "Smart Court"
  }
}
```

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PROD-SQL-SERVER;Database=SmartCourt;User Id=smartcourt_app;Password=***;Encrypt=true"
  },
  "Jwt": {
    "Key": "*** (from environment variable / secret manager)"
  },
  "Cors": {
    "Origins": ["https://smartcourt.eg"]
  },
  "FileStorage": {
    "Provider": "Local",
    "BasePath": "/var/smartcourt/uploads"
  }
}
```

---

## 5. Production Deployment Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    On-Premises Server                     │
│                                                          │
│  ┌─────────────┐    ┌────────────────┐    ┌───────────┐ │
│  │   Nginx      │───▶│  Kestrel (.NET)│───▶│ SQL Server│ │
│  │   (Reverse   │    │  Port 5000     │    │ Port 1433 │ │
│  │   Proxy +    │    └────────────────┘    └───────────┘ │
│  │   SSL/TLS)   │                                        │
│  │   Port 443   │    ┌────────────────┐    ┌───────────┐ │
│  └─────────────┘    │  React (Static) │    │  Qdrant   │ │
│                     │  Served by Nginx│    │  Port 6333│ │
│                     └────────────────┘    └───────────┘ │
│                                                          │
│  ┌─────────────┐    ┌────────────────┐                   │
│  │   Seq        │    │  File Storage  │                   │
│  │   Port 8081  │    │  /var/uploads  │                   │
│  └─────────────┘    └────────────────┘                   │
└──────────────────────────────────────────────────────────┘
```

### Nginx Configuration

```nginx
server {
    listen 443 ssl http2;
    server_name smartcourt.eg;

    ssl_certificate     /etc/ssl/smartcourt.crt;
    ssl_certificate_key /etc/ssl/smartcourt.key;

    # React SPA
    location / {
        root /var/www/smartcourt/frontend;
        try_files $uri $uri/ /index.html;
    }

    # .NET API
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR WebSockets
    location /hubs/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }

    # File downloads
    location /uploads/ {
        alias /var/smartcourt/uploads/;
        add_header X-Content-Type-Options nosniff;
    }
}
```

---

## 6. Logging Strategy

### Serilog Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://localhost:5341" }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/smartcourt-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Structured Log Events

```csharp
// AI call tracking
Log.Information("AI Analysis completed for Case {CaseId}. Tokens: {Tokens}, Time: {ResponseMs}ms",
    caseId, response.TotalTokens, response.ResponseTimeMs);

// Payment tracking
Log.Information("Payment {TransactionId} processed. Amount: {Amount} {Currency}, Status: {Status}",
    txnId, amount, currency, status);

// Auth tracking
Log.Information("User {UserId} logged in from {IpAddress}", userId, ipAddress);
```

---

## 7. Database Migration Strategy

```bash
# Development — create migration
dotnet ef migrations add InitialCreate -p src/SmartCourt.Infrastructure -s src/SmartCourt.API

# Development — apply migration
dotnet ef database update -p src/SmartCourt.Infrastructure -s src/SmartCourt.API

# Production — generate SQL script for DBA review
dotnet ef migrations script -p src/SmartCourt.Infrastructure -s src/SmartCourt.API -o migrations.sql --idempotent

# Production — apply (after DBA review)
sqlcmd -S PROD-SERVER -d SmartCourt -i migrations.sql
```

### Migration Rules
1. **Never** auto-migrate in production
2. Generate idempotent SQL scripts for DBA review
3. Backup database before applying migrations
4. Test migrations against staging copy first

---

## 8. Backup & Recovery

| Asset | Frequency | Retention | Method |
|-------|-----------|-----------|--------|
| SQL Server (full) | Daily at 2:00 AM | 30 days | SQL Server backup job |
| SQL Server (transaction log) | Every 15 minutes | 7 days | Log shipping |
| File uploads | Daily sync | 30 days | rsync to backup drive |
| Qdrant vectors | Weekly | 4 snapshots | Qdrant snapshot API |
| Application logs | Daily rotation | 30 days | Serilog file sink |
