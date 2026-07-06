---
name: add-app-setting
description: Safely adds configuration settings to the project using the strongly-typed IOptions pattern.
---

# Add Application Setting Skill

When the user asks you to add a new configuration setting, API key, toggle, or environment variable, follow these exact steps to ensure type safety and proper DI injection. **Do not** inject `IConfiguration` directly into services.

1. **Create the Options Class**:
   Create a standard C# class to represent the settings.
   - If the setting belongs to external infrastructure (e.g., Database, Payment SDK, Email), place it in `SmartCourt/Providers/<Module>/<SettingName>Options.cs`.
   - If the setting belongs to a specific feature, place it in `SmartCourt/Features/<FeatureName>/<SettingName>Options.cs`.
   - If it's a global API setting, place it in `SmartCourt/Common/Configuration/<SettingName>Options.cs`.
   - Example: 
     ```csharp
     public class FeatureToggleOptions {
         public bool EnableNewMatchingAlgorithm { get; set; }
     }
     ```

2. **Update JSON Configuration Files**:
   Add the JSON block to both `SmartCourt/appsettings.json` and `SmartCourt/appsettings.Development.json`. Make sure the JSON keys exactly match your C# property names.

3. **Register in Dependency Injection**:
   Bind the JSON section to your Options class.
   - Open `SmartCourt/DependencyInjection.cs` (for infrastructure) or `SmartCourt/Program.cs` (for features/global).
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
