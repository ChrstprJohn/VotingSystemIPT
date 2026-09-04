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
   - Contains domain entities mapped to MongoDB documents (e.g., `Candidate`, `Election`, `Vote`) using BSON attributes.
   - Contains ViewModels used for view rendering and form validation.
2. **View (`Views/`)**:
   - Razor templates (`.cshtml`) that generate HTML.
   - Uses data supplied by Controllers via strongly typed models (`@model MyViewModel`).
3. **Controller (`Controllers/`)**:
   - Accepts input from users (GET/POST requests).
   - Coordinates services/repositories, interacts with MongoDB via `MongoDB.Driver`, and selects which view to render.
4. **Data Access / Services (`Services/`)**:
   - Manages `IMongoCollection<T>` operations (Create, Read, Update, Delete).
   - Keeps raw database queries clean and decoupled from controller actions.

---

## 2. Directory Layout

```text
VotingSystem/
├── .DOCUMENTS/          # Project documentation (Infrastructures, Techstacks, Rules)
├── Controllers/         # Action methods & request handlers
├── Models/              # Domain models (BSON entities) & ViewModels
├── Services/            # MongoDB data access and business services
├── Views/               # Razor templates (.cshtml)
│   ├── Home/            # Views specific to HomeController
│   │   ├── Index.cshtml # Main page (Orchestrator view)
│   │   ├── About.cshtml
│   │   ├── Contact.cshtml
│   │   └── IndexPartials/ # Feature-scoped partial views
│   │       ├── _Hero.cshtml
│   │       ├── _Features.cshtml
│   │       └── _CTA.cshtml
│   └── Shared/          # Shared views (_Layout.cshtml, _Navbar.cshtml, _Footer.cshtml, _ValidationScriptsPartial.cshtml)
│       ├── _Layout.cshtml
│       ├── _Navbar.cshtml
│       └── _Footer.cshtml
├── wwwroot/             # Static web assets
│   ├── css/             # Modular stylesheets (Option 1 architecture)
│   │   ├── base/        # Design tokens & resets (variables.css, reset.css)
│   │   ├── components/  # Reusable UI components (navbar.css, footer.css)
│   │   ├── pages/       # Page-specific stylesheets (home.css)
│   │   └── site.css     # Central entry point importing base and components
│   ├── js/              # Custom scripts (site.js)
│   └── lib/             # Third-party dependencies (Bootstrap, jQuery)
├── Program.cs           # Web application bootstrap, DI (MongoDB registration), & middleware
├── VotingSystem.csproj  # Project settings, target framework, & NuGet packages
└── appsettings.json     # Application settings & MongoDB connection strings
```
