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

## 2. Architecture & Pipeline
- **Entry Point**: `Program.cs`
- **Middleware Pipeline**:
  - `UseExceptionHandler` & `UseHsts` in non-development modes.
  - `UseHttpsRedirection`: Automatically forwards HTTP requests to secure HTTPS.
  - `UseRouting`: Resolves incoming endpoints.
  - `UseAuthorization`: Secures controllers/actions based on user credentials.
  - `MapStaticAssets` & `MapControllerRoute`: Maps default route `{controller=Home}/{action=Index}/{id?}`.

---

## 3. Core MVC Responsibilities
- **Controllers (`Controllers/`)**:
  - Inherits from `Microsoft.AspNetCore.Mvc.Controller`.
  - Handles incoming HTTP requests, orchestrates business logic, and returns Views or JSON.
- **Models (`Models/`)**:
  - Represents database entities and ViewModels (e.g., `ErrorViewModel.cs`).
  - Implements DataAnnotations (`[Required]`, `[StringLength]`, etc.) for validation.
