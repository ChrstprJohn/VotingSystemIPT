# Coding Standards & Guidelines

This document outlines best practices for C#, Razor Views, and CSS in this project.

---

## 1. C# & Controller Guidelines
- **PascalCase**: Use for classes, methods, properties, and constants (`CandidateController`, `VoteCount`).
- **camelCase**: Use for local variables and method parameters (`candidateId`, `voterName`).
- **Keep Controllers Thin**: Controllers should only handle input, call necessary services/models, and return a view or redirect. Avoid heavy calculation logic directly in action methods.
- **Model Validation**: Always validate models in POST actions:
  ```csharp
  if (!ModelState.IsValid)
  {
      return View(model);
  }
  ```

---

## 2. Razor View Guidelines (`.cshtml`)
- **Strongly-Typed Views**: Always declare `@model YourNamespace.Models.YourViewModel` at the top of views to gain compile-time checks and autocomplete.
- **Orchestrator & Partial View Pattern**:
  - Keep main views (`Index.cshtml`) clean by treating them as **orchestrators**.
  - Break down discrete sections into a dedicated `{Action}Partials/` subdirectory (e.g. `IndexPartials/_Hero.cshtml`, `IndexPartials/_Features.cshtml`).
  - Reference partials via the Tag Helper: `<partial name="IndexPartials/_PartialName" />`.
- **Tag Helpers**: Prefer ASP.NET Core Tag Helpers over raw HTML links or old HTML helpers:
  - Good: `<a asp-controller="Vote" asp-action="Index">Vote Now</a>`
  - Good: `<input asp-for="CandidateName" class="form-control" />`
  - Avoid: `<a href="/Vote/Index">Vote Now</a>`

---

## 3. CSS & Styling Guidelines
- **Modular CSS Architecture**:
  - Store reusable design tokens (colors, font families, radiuses, shadows) in `wwwroot/css/base/variables.css`.
  - Place shared layout component styles in `wwwroot/css/components/` (e.g., `navbar.css`, `footer.css`) and import them in `wwwroot/css/site.css`.
  - Place page-heavy, route-specific styles in `wwwroot/css/pages/` (e.g., `home.css`) and inject them via `@section Styles` in the view.
- **Bootstrap First, Custom CSS for Polish**:
  - Use Bootstrap utility classes (`d-flex`, `mb-3`, `text-center`, `row`, `col-md-6`) for structure and grid alignment.
  - Rely on CSS Custom Properties (`var(--color-primary)`) rather than hardcoded hex values.
- **BEM or Descriptive Class Naming**:
  - Use kebab-case or BEM for custom CSS classes: `.home-hero`, `.home-cta`, `.main-navbar`, `.main-footer`.
- **Responsive Design**: Always test views on both mobile and desktop viewports.
