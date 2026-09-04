# Infrastructure & Environment

This document defines the local setup, tooling, environment configurations, and hosting requirements.

---

## 1. System Requirements & Tooling
- **.NET SDK**: .NET 10 SDK or higher.
- **Recommended IDE**: Visual Studio Community 2026 or VS Code with C# Dev Kit.
- **Solution File**: `VotingSystem.slnx`
- **Project File**: `VotingSystem.csproj`

---

## 2. Configuration Files
The application uses ASP.NET Core configuration providers:
- **`appsettings.json`**: Base configuration (Logging, connection strings, feature flags).
- **`appsettings.Development.json`**: Overrides specific to local development (e.g., debug logging levels).

### MongoDB Configuration Keys:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "VotingSystemDb"
  }
}
```
*Note: For production or sensitive credentials, use `dotnet user-secrets` in development or environment variables in production.*

---

## 3. Local Execution Commands
All commands run from the `VotingSystem/` directory:

| Action | Command |
| :--- | :--- |
| **Restore Dependencies** | `dotnet restore` |
| **Build Project** | `dotnet build` |
| **Run Server** | `dotnet run` |
| **Run with Hot Reload** | `dotnet watch` |

---

## 4. Static Asset Hosting
- All public static files are located in `wwwroot/`.
- Served via `app.MapStaticAssets()` in `Program.cs`.
- Bundled third-party libraries live under `wwwroot/lib/` (Bootstrap, jQuery).
