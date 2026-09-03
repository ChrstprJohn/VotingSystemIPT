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
- **Tag Helpers**: Prefer ASP.NET Core Tag Helpers over raw HTML links or old HTML helpers:
  - Good: `<a asp-controller="Vote" asp-action="Index">Vote Now</a>`
  - Good: `<input asp-for="CandidateName" class="form-control" />`
  - Avoid: `<a href="/Vote/Index">Vote Now</a>`

---

## 3. CSS & Styling Guidelines
- **Bootstrap First, Custom CSS for Polish**:
  - Use Bootstrap utility classes (`d-flex`, `mb-3`, `text-center`, `row`, `col-md-6`) for layout and alignment.
  - Write custom styles in `wwwroot/css/site.css` for distinct visual branding, card elevation, custom themes, gradients, and animations.
- **BEM or Descriptive Class Naming**:
  - Use kebab-case for custom CSS classes: `.voting-card`, `.candidate-avatar`, `.vote-tally-badge`.
- **Responsive Design**: Always test views on both mobile and desktop viewports.
