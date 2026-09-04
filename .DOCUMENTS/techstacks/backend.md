# Tech Stack: Backend Specifications

This document outlines the core backend technologies, language standards, and server dependencies.

---

## 1. Runtime & Framework
- **Runtime Target**: .NET 10 (`net10.0`)
- **SDK**: `Microsoft.NET.Sdk.Web`
- **Application Type**: ASP.NET Core MVC (Model-View-Controller)
- **Language**: C# 13+
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)

---

## 2. Database & Data Access Layer
- **Database Engine**: MongoDB (NoSQL Document Store)
- **Official Driver / ODM**: `MongoDB.Driver` (v3.x+)
- **Configuration Pattern**:
  - Store connection credentials in `appsettings.json` / `appsettings.Development.json` under `MongoDbSettings`:
    ```json
    "MongoDbSettings": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "VotingSystemDb"
    }
    ```
- **Object-Document Mapping (BSON)**:
  - Models map directly to Mongo collections using BSON attributes (`[BsonId]`, `[BsonRepresentation(BsonType.ObjectId)]`, `[BsonElement("...")]`).
  - Queries leverage LINQ expressions via `IMongoCollection<T>.Find(...)` and `Builders<T>`.
- **Dependency Injection**:
  - `IMongoClient` registered as a Singleton (`builder.Services.AddSingleton<IMongoClient>(...)`).
  - Database services or repositories registered as Scoped or Singleton based on needs.

---

## 3. Architecture & Pipeline
- **Entry Point**: `Program.cs`
- **Middleware Pipeline**:
  - `UseExceptionHandler` & `UseHsts` in non-development modes.
  - `UseHttpsRedirection`: Automatically forwards HTTP requests to secure HTTPS.
  - `UseRouting`: Resolves incoming endpoints.
  - `UseAuthorization`: Secures controllers/actions based on user credentials.
  - `MapStaticAssets` & `MapControllerRoute`: Maps default route `{controller=Home}/{action=Index}/{id?}`.

---

## 4. Core MVC Responsibilities
- **Controllers (`Controllers/`)**:
  - Inherits from `Microsoft.AspNetCore.Mvc.Controller`.
  - Handles incoming HTTP requests, coordinates data access via MongoDB services, and returns Views or JSON.
- **Models (`Models/`)**:
  - Represents MongoDB document entities and ViewModels (e.g., `Candidate.cs`, `Election.cs`, `Vote.cs`).
  - Implements DataAnnotations (`[Required]`, `[StringLength]`, etc.) for validation and Bson attributes for Mongo serialization.
- **Services / Data Access (`Services/` or `Data/`)**:
  - Encapsulates MongoDB collections (`IMongoCollection<T>`) and CRUD operations away from controllers.
