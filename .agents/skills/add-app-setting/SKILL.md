---
name: add-app-setting
description: Safely adds configuration settings to the project using the strongly-typed IOptions pattern.
---

# Add Application Setting Skill

When the user asks you to add a new configuration setting, API key, toggle, or environment variable, follow these exact steps to ensure type safety and proper DI injection. **Do not** inject `IConfiguration` directly into services.

1. **Create the Options Class**:
   Create a standard C# class to represent the settings.
   - If the setting belongs to Infrastructure (e.g., Database, Payment SDK, Email), place it in `SmartCourt.Infrastructure/Providers/<Module>/<SettingName>Options.cs`.
   - If the setting belongs to the API (e.g., JWT, Rate Limiting), place it in `SmartCourt.API/Configuration/<SettingName>Options.cs`.
   - Example: 
     ```csharp
     public class FeatureToggleOptions {
         public bool EnableNewMatchingAlgorithm { get; set; }
     }
     ```

2. **Update JSON Configuration Files**:
   Add the JSON block to both `SmartCourt.API/appsettings.json` and `SmartCourt.API/appsettings.Development.json`. Make sure the JSON keys exactly match your C# property names.

3. **Register in Dependency Injection**:
   Bind the JSON section to your Options class.
   - For Infrastructure settings, open `SmartCourt.Infrastructure/DependencyInjection.cs`.
   - For API settings, open `SmartCourt.API/Extensions/ApplicationBuilderExtensions.cs`.
   - Add: `services.Configure<<SettingName>Options>(configuration.GetSection("SectionNameInJson"));`

4. **Inject via IOptions**:
   In whatever Service or Provider needs these settings, inject the `IOptions<T>` interface, NOT `IConfiguration`.
   - Example:
     ```csharp
     public class MyService
     {
         private readonly FeatureToggleOptions _options;
         public MyService(IOptions<FeatureToggleOptions> options)
         {
             _options = options.Value;
         }
     }
     ```
