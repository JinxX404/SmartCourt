# Smart Court

Smart Court is a modern, enterprise-grade backend API built on **.NET 8**. The project emphasizes maintainability, scalability, and clean code practices. It leverages a hybrid approach combining **Clean Architecture** principles for the core domain and infrastructure separation, along with a **Vertical Slice Architecture** for feature organization within the API.

## 🏗 Architecture & Design Patterns

The solution is divided into three main projects to enforce boundary separation and dependency rules:

1. **SmartCourt.Core**
   - The heart of the application containing Domain Entities (`BaseEntity`, `AuditableEntity`), Common utilities, and custom Exceptions (`BusinessException`, `NotFoundException`).
   - Defines provider interfaces (`IEmailProvider`, `ISmsProvider`, `IBackgroundJobProvider`) that infrastructure must implement.
   - **No external dependencies** (no database or framework specific code).

2. **SmartCourt.Infrastructure**
   - Implements the interfaces defined in Core using external tools and SDKs.
   - Contains the **Provider Pattern** implementations:
     - **Email**: `SmtpEmailSender` (via MailKit) / `MockSmtpEmailSender`
     - **SMS**: `TwilioSmsSender` (via Twilio) / `MockSmsSender`
     - **Background Jobs**: `HangfireJobProvider` (via Hangfire)
   - Persistence layer using **Entity Framework Core** (`ApplicationDbContext`) and database migrations.

3. **SmartCourt.API**
   - Organizes endpoints using **Vertical Slice Architecture**. Instead of separating by technical concern (Controllers, Services, Models), it organizes by feature (e.g., `Features/Test`).
   - Each feature slice is self-contained with its own Controller, Service, DTOs, and Validators.
   - Features standard `ApiResponse<T>` wrapper and Global Exception Handling Middleware.

## 🛠 Tech Stack

- **Framework:** .NET 8.0
- **Database:** Entity Framework Core & SQL Server
- **Background Jobs:** Hangfire
- **Email:** MailKit / SMTP
- **SMS:** Twilio
- **Validation:** FluentValidation

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server (LocalDB or full instance)
- A preferred IDE (Visual Studio, Rider, VS Code)

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd "Smart Court"
   ```

2. **Restore Packages and Build**
   Ensure all NuGet packages are restored and the solution builds successfully:
   ```bash
   dotnet restore
   dotnet build
   ```
   *To add new packages during development, use `dotnet add package <PackageName>` inside the respective project folder.*

3. **Configure AppSettings**
   Open `SmartCourt.API/appsettings.json` and update the settings with your local configurations or service keys:
   - `ConnectionStrings:DefaultConnection`: Ensure it points to your SQL Server instance.
   - `SmtpSettings`: Configure your SMTP server details for sending emails.
   - `Twilio`: Add your Account SID, Auth Token, and From Number for SMS functionality.

4. **Apply Database Migrations**
   The application uses EF Core Code-First migrations. From the solution root folder, run:
   ```bash
   dotnet ef database update --project SmartCourt.Infrastructure --startup-project SmartCourt.API
   ```

5. **Run the Application**
   ```bash
   dotnet run --project SmartCourt.API
   ```
   The API will launch and be accessible typically at `http://localhost:<port>` or `https://localhost:<port>`. Swagger UI is usually available at `/swagger` for endpoint exploration.

## 📝 Development Guidelines

When contributing to this project, adhere strictly to the following rules:

### 1. Vertical Slice (API Layer)
- **Slice Structure:** Create new features inside `SmartCourt.API/Features/FeatureName`. This folder should contain its own `Controller`, `Service`, `I{Feature}Service`, `DTOs/`, and `Validators/`.
- **Cross-Communication:** If a feature needs logic from another, inject the Service Interface. **Never** inject controllers.
- **No CQRS/MediatR:** Use simple Service Classes for business logic.

### 2. API Responses & Exceptions
- **Standardized Responses:** Always return an `ApiResponse<T>` (e.g., `ApiResponse<T>.Ok(data)` or `ApiResponse<T>.Created(data)`).
- **Exceptions:** Throw specific domain exceptions like `BusinessException` or `NotFoundException`. Do not return 500 status codes manually from controllers; let the `ExceptionHandlingMiddleware` catch and format them.

### 3. Implementation Details
- **Mapping:** Map Entities to DTOs manually within the service class. Do **not** use AutoMapper.
- **Validation:** Use **FluentValidation** inside the feature's `Validators/` folder. Do **not** use Data Annotations on DTOs.
- **Async:** Always use `async`/`await`. Avoid `.Result` or `.Wait()`.

### 4. Database & Infrastructure
- Keep all EF Core Configurations (Fluent API) inside `Infrastructure/Persistence/Configurations`. Do not put Data Annotations on Core Entities.
- **External Dependencies:** Do not directly instantiate SDKs inside feature services. Instead, create a Provider Interface in Core and implement it in Infrastructure. Use Dependency Injection.

## 🧩 Dependency Injection
- Register core feature services in API Extensions (`SmartCourt.API/Extensions/ApplicationBuilderExtensions.cs`).
- Register Infrastructure components (Db, Providers) in `SmartCourt.Infrastructure/DependencyInjection.cs`.
