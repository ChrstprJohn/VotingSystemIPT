# System Architecture: ASP.NET Core MVC

This document details how the VotingSystem application is structured and how data flows through the system.

---

## 1. Architectural Pattern: Model-View-Controller (MVC)

```
        HTTP Request
             │
             ▼
     ┌───────────────┐
     │  Controller   │ ◄──── Route matching (e.g. /Home/Index)
     └───────┬───────┘
             │
     ┌───────┴───────┐
     ▼               ▼
┌─────────┐     ┌─────────┐
│  Model  │     │  View   │ (Razor .cshtml + CSS)
└─────────┘     └────┬────┘
                     │
                     ▼
             Rendered HTML/CSS to Browser
```

1. **Model (`Models/`)**:
   - Contains business logic, domain entities (e.g., `Candidate`, `Election`, `Vote`), and ViewModels.
   - Encapsulates data and validation rules.
2. **View (`Views/`)**:
   - Razor templates (`.cshtml`) that generate HTML.
   - Uses data supplied by Controllers via strongly typed models (`@model MyViewModel`).
3. **Controller (`Controllers/`)**:
   - Accepts input from users (GET/POST requests).
   - Coordinates models, database queries, and selects which view to render.

---

## 2. Directory Layout

```text
VotingSystem/
├── .DOCUMENTS/          # Project documentation (Infrastructures, Techstacks, Rules)
├── Controllers/         # Action methods & request handlers
├── Models/              # Domain models & ViewModels
├── Views/               # Razor templates (.cshtml)
│   ├── Home/            # Views specific to HomeController
│   └── Shared/          # Shared views (_Layout.cshtml, _ValidationScriptsPartial.cshtml)
├── wwwroot/             # Static web assets
│   ├── css/             # Custom stylesheets (site.css)
│   ├── js/              # Custom scripts (site.js)
│   └── lib/             # Third-party dependencies (Bootstrap, jQuery)
├── Program.cs           # Web application bootstrap and middleware configuration
├── VotingSystem.csproj  # Project settings and target framework
└── appsettings.json     # Application settings & connection strings
```
