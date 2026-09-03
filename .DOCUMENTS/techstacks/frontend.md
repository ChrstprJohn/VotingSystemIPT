# Tech Stack: Frontend Specifications

This document defines the client-facing technologies and libraries currently installed and configured in the project.

---

## 1. Templating Engine: Razor Views (`.cshtml`)
- **Technology**: ASP.NET Core Razor
- **File Extensions**: `.cshtml`
- **Location**: `Views/`
  - `Views/Shared/_Layout.cshtml`: Master layout wrapping all views.
  - `Views/{Controller}/{Action}.cshtml`: Individual page templates.
- **Role**: Server-Side Rendering (SSR). Embeds dynamic C# values into standard HTML at request time using the `@` symbol.
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

## 3. Custom Styling: Vanilla CSS
- **Primary File**: `wwwroot/css/site.css`
- **Scoped Styles**: `VotingSystem.styles.css` (ASP.NET Core CSS Isolation)
- **Usage**:
  - Overrides Bootstrap styles.
  - Custom design tokens, color schemes, typography, card layouts, animations, and transitions.
  - Modern CSS features: CSS Grid, Flexbox, CSS Custom Properties (Variables), and Media Queries.

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
