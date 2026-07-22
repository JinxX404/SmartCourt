# Environment Settings & Database Setup Guide

This guide explains how local and hosted development configurations are structured in the Smart Court project, how to seamlessly swap between them, and how to run Entity Framework (EF) Core migrations against the correct database.

## 1. App Settings Files

To prevent sensitive secrets (like database passwords, API keys, and JWT secrets) from leaking into the GitHub repository, our local configuration files are explicitly ignored in `.gitignore`. 

When setting up your development environment for the first time, you must create these two files in the root of the `SmartCourt` project:

*   **`appsettings.Development.json`**: Contains your local development settings (e.g., Local SQL Server connection string).
*   **`appsettings.Development.Hosted.json`**: Contains connection strings and settings that point to the remote Hosted database and services.

*(Note: Ask the team lead or check your secure team password manager for the actual JSON contents of these files).*

## 2. Swapping Environments in Your IDE

You don't need to manually edit JSON files to switch between your local database and the hosted database.

We use **Launch Profiles** configured in `Properties/launchSettings.json`.

In Visual Studio or JetBrains Rider:
1. Locate the **Run / Debug Dropdown** button at the top of your IDE.
2. Select **"Local DB"** to run the app using `appsettings.Development.json`.
3. Select **"Hosted DB"** to run the app using `appsettings.Development.Hosted.json`.

## 3. Running EF Core Migrations

Because we use multiple environments, you **must explicitly specify** which environment you want to target when pushing database updates using Entity Framework tools.

### Updating the Local Database
To apply migrations to your local development database, use the `Development` environment flag:

```bash
dotnet ef database update -p SmartCourt --environment Development
```

### Updating the Hosted Database
To apply migrations directly to the hosted development database, use the `Development.Hosted` environment flag:

```bash
dotnet ef database update -p SmartCourt --environment Development.Hosted
```

### Adding New Migrations
When generating a new migration based on your code changes, you typically use your local environment:
```bash
dotnet ef migrations add "YourMigrationName" -p SmartCourt --environment Development
```

> [!WARNING]  
> Always double-check your `--environment` flag before running an `update` to ensure you aren't accidentally pushing unfinished schema changes to the hosted environment!

## 4. MVP Hosting Topology

The MVP API is directly hosted and is not behind a reverse proxy. Forwarded headers are intentionally not configured, so IP-based security controls use the client's direct connection address.

If the deployment topology changes, configure forwarded headers only for known trusted proxies and place that middleware before authentication and IP-based rate limiting.
