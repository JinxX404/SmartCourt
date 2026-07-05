# Smart Court Agent Rules

When writing code or assisting with the Smart Court project, ALWAYS follow these architectural constraints and best practices:

## Architecture: Vertical Slice + Service Layer
1. **API Structure**: We use Vertical Slice Architecture. The API is organized by Feature slices in `SmartCourt.API/Features`.
2. **Slice Contents**: Each feature folder (e.g., `Features/Proposals`) must contain its own Controller, Service (`I{Feature}Service`, `{Feature}Service`), DTOs folder, and Validators folder. Do not share these components across features.
3. **No CQRS/MediatR**: Do NOT use MediatR. Use simple Service Classes for business logic.
4. **Cross-Feature Communication**: If slice A needs data/logic from slice B, inject slice B's Service interface into slice A's Service. DO NOT inject controllers.

## API & Responses
5. **Standardized Responses**: All controller endpoints must return responses wrapped in `ApiResponse<T>` (e.g., `ApiResponse<T>.Ok(data)` or `ApiResponse<T>.Created(data)`).
6. **Exception Handling**: Throw `BusinessException` for domain errors (e.g., `throw new BusinessException("Error message");`). Let the global middleware catch and format these. Do not manually return `500` status codes.

## Implementation Details ("Use X, Not Y")
7. **Validation**: Use FluentValidation inside the `Validators/` folder of the slice. DO NOT use Data Annotations on DTOs.
8. **Mapping**: DO NOT use AutoMapper. Map entities to DTOs manually within your service classes (e.g. `private static MyDto MapToDto(Entity e)`).
9. **External Dependencies**: Use the **Provider Pattern** defined in Core. Do not instantiate external SDKs in services directly. Use `IEmailProvider` (implemented by MailKit), `IVectorStoreProvider` (Qdrant), etc.
10. **Database**: Use Entity Framework Core with SQL Server. Keep all EF Core Configurations (Fluent API) inside `Infrastructure/Persistence/Configurations`. Do not use Data Annotations on Core Entities.
11. **Async Code**: Always use `async` and `await`. Never use `.Result` or `.Wait()`.

## Dependency Injection
12. **Infrastructure/Providers**: Register new infrastructure implementations or DbContexts in `SmartCourt.Infrastructure/DependencyInjection.cs`.
13. **Features/Services**: Register feature services (e.g., `services.AddScoped<ICaseService, CaseService>()`) in the API's DI extensions.
