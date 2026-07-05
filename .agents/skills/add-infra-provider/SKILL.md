---
name: add-infra-provider
description: Scaffolds a new external infrastructure provider (e.g., SMS, Payment, Storage) strictly following the Provider Pattern.
---

# Add Infrastructure Provider Skill

When the user asks to add a new external integration or "provider" (like a new payment gateway, SMS service, or third-party API) to the Smart Court project, follow these exact steps to ensure the business logic remains decoupled from the external SDK:

1. **Create the Interface (Core Layer)**:
   Create the interface in `SmartCourt.Core/Interfaces/Providers/I<ProviderName>Provider.cs`.
   - Ensure the interface uses domain terminology, not third-party specific terminology.
   - Example: `Task<bool> SendSmsAsync(string phoneNumber, string message);`

2. **Create the Options Class (Infrastructure Layer)**:
   Create a settings class in `SmartCourt.Infrastructure/Providers/<Category>/<ProviderName>Options.cs` to hold configuration values (API Keys, URLs).
   - Example: `TwilioOptions.cs` with properties for `AccountSid` and `AuthToken`.
   - Create the `<Category>` folder (e.g., `Sms`, `Payment`, `Storage`) if it doesn't already exist.

3. **Create the Implementation (Infrastructure Layer)**:
   Create the implementation class in `SmartCourt.Infrastructure/Providers/<Category>/<ProviderName>Provider.cs` that implements the interface.
   - Inject `IOptions<<ProviderName>Options>` and `ILogger<<ProviderName>Provider>`.
   - This is the ONLY place where you should use the external SDK/Library.

4. **Register the Provider (Dependency Injection)**:
   Open `SmartCourt.Infrastructure/DependencyInjection.cs` and add the provider inside the `AddInfrastructureServices` method:
   ```csharp
   services.Configure<<ProviderName>Options>(configuration.GetSection("<ProviderName>"));
   services.AddScoped<I<ProviderName>Provider, <ProviderName>Provider>();
   ```

5. **Update appsettings.json**:
   Remind the user to add the corresponding configuration block to `SmartCourt.API/appsettings.json` (and optionally add a placeholder block yourself).
   ```json
   "<ProviderName>": {
     "ApiKey": "YOUR_API_KEY"
   }
   ```

6. **Review Rules**:
   Ensure no external third-party namespaces or SDKs leaked into the `SmartCourt.Core` project or the `SmartCourt.API` vertical slices. All usage of this provider in feature slices MUST go through the injected `I<ProviderName>Provider` interface.
