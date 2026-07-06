---
name: create-vertical-slice
description: Scaffolds a new vertical slice for the Smart Court API including Controller, Service, DTOs, and Validators.
---

# Create Vertical Slice Skill

When the user asks you to create a new feature or a new vertical slice in the Smart Court API, follow these steps exactly:

1. **Create the Folder Structure**: 
   Create a new directory under `SmartCourt/Features/<FeatureName>`.
   Inside it, create `DTOs/` and `Validators/` subdirectories.

2. **Create the DTOs**:
   Create request and response models inside `DTOs/` (e.g., `Create<FeatureName>Request.cs`, `<FeatureName>Response.cs`).

3. **Create the Validators**:
   Use FluentValidation inside the `Validators/` directory (e.g., `Create<FeatureName>RequestValidator.cs`). Ensure you add standard rules and do NOT use Data Annotations in the DTOs.

4. **Create the Service Interface**:
   Create `I<FeatureName>Service.cs` in the root of the feature folder. Define the contract using `Task` or `Task<T>`.

5. **Create the Service Implementation**:
   Create `<FeatureName>Service.cs` implementing the interface. Inject `ApplicationDbContext` or any required Providers. Do NOT use AutoMapper; write manual mapping logic to map entities to `ApiResponse<T>`.

6. **Create the Controller**:
   Create `<FeatureName>Controller.cs`. Inject the service interface (`I<FeatureName>Service`). Create standard HTTP endpoints (`[HttpGet]`, `[HttpPost]`, etc.). Make sure every endpoint returns `ApiResponse<T>` (e.g., `ApiResponse<T>.Ok(...)`).

7. **Register the Service**:
   Open the API DI setup (usually `ApplicationBuilderExtensions.cs` or `Program.cs`) and register the new service: `services.AddScoped<I<FeatureName>Service, <FeatureName>Service>();`.

8. **Review Rules**:
   Ensure you followed all standard architecture rules (No MediatR, no `[Required]` in DTOs, etc.).
