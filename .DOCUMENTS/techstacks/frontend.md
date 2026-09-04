# Tech Stack: Frontend Specifications

This document defines the client-facing technologies and libraries currently installed and configured in the project.

---

## 1. Templating Engine: Razor Views (`.cshtml`)
- **Technology**: ASP.NET Core Razor
- **File Extensions**: `.cshtml`
- **Location**: `Views/`
  - `Views/Shared/_Layout.cshtml`: Master layout wrapping all views.
  - `Views/{Controller}/{Action}.cshtml`: Individual page templates acting as **Orchestrators**.
  - `Views/{Controller}/{Action}Partials/`: Feature-scoped partial templates (`_SectionName.cshtml`).
- **Role**: Server-Side Rendering (SSR). Embeds dynamic C# values into standard HTML at request time using the `@` symbol.
- **Frontend View Pattern (Orchestrator + Partials)**:
  - Complex views are broken down into cohesive, self-contained partial views rather than monolithic files.
  - Example:
    ```text
    Views/Home/
    ├── Index.cshtml               --> Orchestrator (assembles layout & sections)
    └── IndexPartials/             --> Feature-scoped partials
        ├── _Hero.cshtml
        ├── _Features.cshtml
        └── _CTA.cshtml
    ```
  - Orchestrators include partials using `<partial name="IndexPartials/_Section" />` tag helpers.
- **Notice on SPA / React**: 
  - React is **NOT** used in this setup to adhere directly to ASP.NET Core MVC requirements. Razor generates the markup directly on the server.

---

## 2. CSS Framework: Bootstrap 5
- **Library**: Twitter Bootstrap (v5.x)
- **Asset Location**: `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css`
- **Script Location**: `wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js`
- **Loaded In**: `Views/Shared/_Layout.cshtml`
- **Usage**:
  - Grid system (`container`, `row`, `col-*`) for responsive layouts.
  - Pre-styled UI components: navigation bars (`navbar`), buttons (`btn`, `btn-primary`), cards (`card`), modals, forms, and badges.

---

## 3. Custom Styling: Modular Vanilla CSS (Option 1 Standard)
- **Primary Master File**: `wwwroot/css/site.css` (central entry point importing base and component layers)
- **Base Layer (`wwwroot/css/base/`)**:
  - `variables.css`: Design tokens (CSS custom properties for colors, typography, spacing, radius, elevation).
  - `reset.css`: Global resets, box-sizing, and typography defaults.
- **Components Layer (`wwwroot/css/components/`)**:
  - `navbar.css`: Navigation bar styling.
  - `footer.css`: Global footer styling.
  - Additional reusable component styles (buttons, cards, forms).
- **Pages Layer (`wwwroot/css/pages/`)**:
  - View-specific stylesheets (e.g., `home.css`) injected via `@section Styles` in Razor views.
- **Scoped Styles**: `VotingSystem.styles.css` (ASP.NET Core native CSS Isolation support).
- **Usage**:
  - Overrides Bootstrap styles while preserving responsive grid utilities.
  - Consistent design tokens and CSS custom properties (`var(--color-primary)`).
  - Modern CSS features: Flexbox, CSS Grid, custom properties, and transitions.

---

## 4. Client-Side JavaScript & DOM Manipulation
- **Primary Custom File**: `wwwroot/js/site.js`
- **Libraries**:
  - **jQuery**: Located at `wwwroot/lib/jquery/dist/jquery.min.js`.
  - **jQuery Validation**: Located at `wwwroot/lib/jquery-validation/dist/jquery.validate.min.js`.
  - **Unobtrusive Validation**: Located at `wwwroot/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js`.
- **Usage**:
  - Real-time client-side form validation matching DataAnnotations in C# Models.
  - Client-side interactivity (toggles, confirmations, alerts).
