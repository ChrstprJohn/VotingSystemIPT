# CLAUDE.md
# QCU VotingSystem — Front-End Design System

This file documents the design system **as actually implemented** in this
ASP.NET Core MVC project. It is derived from `wwwroot/css/site.css` (global
tokens and base styles) and `wwwroot/css/Login.css` (the admin login screen,
the first fully styled page). Keep this file in sync with those files — if a
rule here and the CSS disagree, the CSS is the source of truth and this file
should be corrected.

## Project Structure
```
VotingSystemIPT/
├── wwwroot/
│   ├── CLAUDE.md              <- this file
│   ├── css/
│   │   ├── site.css           <- :root design tokens + global base styles (loaded on every page)
│   │   └── Login.css          <- page-specific styles for the admin login screen
│   ├── js/
│   │   └── site.js
│   ├── lib/                   <- client libraries restored by libman (bootstrap, jquery, ...)
│   └── favicon.ico
├── Views/
│   ├── Account/
│   │   └── Login.cshtml       <- consumes Login.css via @section Styles
│   └── Shared/
│       └── _LayoutAuth.cshtml <- auth layout: loads bootstrap.min.css, site.css, VotingSystem.styles.css
├── Controllers/
├── Models/
└── Program.cs
```

There is **no `fonts/` folder and no `styles/global.css`**. Global tokens and
base element styles live in `wwwroot/css/site.css`. Bootstrap 5 is loaded
before `site.css`, so `site.css` (and any page CSS) overrides Bootstrap.

---

## Fonts

**Use the native system font stack. Do not add Google Fonts, other font CDNs,
or bundled font files.** There is no local font pipeline in this project.

```css
font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
             "Helvetica Neue", Arial, sans-serif;
```

- All text is this one sans-serif stack. There is no separate display/serif face.
- Weights in use: **400** (regular), **600** (semibold), **700** (bold).
  Avoid 300 and 500.
- Root font size is set on `html` in `site.css`: **14px**, rising to **16px**
  at `min-width: 768px`. Because of this, always size type and spacing in
  `rem`, not `px`, so both breakpoints scale correctly.

---

## Color Palette

### Design tokens (defined in `wwwroot/css/site.css` `:root`)
```css
:root {
  --color-blue-light:        #E8F0F8;  /* very light blue tint */
  --color-blue-accent:       #5B9FBD;  /* primary accent / links / focus ring */
  --color-blue-accent-dark:  #4A7FA0;  /* accent hover / gradient end / active */
  --color-blue-dark:         #2C3E50;  /* primary text, dark UI */
  --color-white-dirty:       #F5F3F0;  /* warm page background */
  --color-gray-soft:         #A0A0A0;  /* secondary text, disabled */
  --color-gray-border:       #E0E0E0;  /* borders, dividers */
  --color-white:             #FFFFFF;  /* cards, overlays */
}
```

**Always reference these via `var(--token)` for the core brand blues,
backgrounds, and borders.** Do not reintroduce brand hexes like `#5B9FBD`
literally in component CSS.

### Supporting neutrals and semantic colors (used in `Login.css`)
The login screen needed a finer neutral ramp and a couple of gradients than the
eight core tokens provide. These are currently allowed as literals in
page-level CSS. If a value below starts appearing on a second page, promote it
to a `--token` in `site.css` first.

| Purpose                        | Value                                            |
|--------------------------------|--------------------------------------------------|
| Muted page backdrop            | `#f0f2f5`, `#f8fafc`                              |
| Neutral text (labels, meta)    | `#334155`, `#475569`, `#64748b`                   |
| Neutral icon / placeholder     | `#94a3b8`                                         |
| Neutral borders / hairlines    | `#e2e8f0`, `#eef1f5`                              |
| Error text / validation        | `#dc2626`                                         |
| Accent-on-dark (brand panel)   | `#9ec9db`                                         |
| Brand-panel gradient stops     | `#24435f`, `#2c3e50`, `#1b2c3d`                   |

### Gradients
Gradients **are permitted** for large brand surfaces and the primary call to
action. Two are in use:
- Brand panel: `linear-gradient(135deg, #24435f 0%, #2c3e50 50%, #1b2c3d 100%)`
- Primary submit button:
  `linear-gradient(to right, var(--color-blue-accent) 0%, var(--color-blue-accent-dark) 100%)`

Do not add gradients to small elements, cards, inputs, or body backgrounds.

### Usage rules
- Page background: `--color-white-dirty` (global) or a light neutral (`#f0f2f5`) for full-bleed screens.
- Cards / panels: `--color-white`.
- Primary button: accent → accent-dark gradient, white text.
- Links & focus rings: `--color-blue-accent`.
- Borders: `--color-gray-border` globally; the finer `#e2e8f0` hairline is acceptable inside a styled card.
- Body text: `--color-blue-dark`. Secondary text: a slate neutral (`#64748b`) or `--color-gray-soft`.

---

## Typography Scale

Sizes are `rem` against the 14/16px root. Values seen in `Login.css`:

| Role                         | Size                | Weight | Notes                       |
|------------------------------|---------------------|--------|-----------------------------|
| Brand hero title             | `3rem`              | 700    | brand panel only            |
| Card title (`h1`/`h2`)       | `1.75rem`           | 700    |                             |
| Brand subtitle               | `1.5rem`            | 600    |                             |
| Body / paragraph             | `1rem`              | 400    | line-height ~1.6            |
| Label / small UI text        | `0.875rem`          | 600    | form labels, feature titles |
| Sub-label / helper           | `0.9375rem`         | 400/600| card subtitle, button text  |
| Caption / footer / error     | `0.8125rem`–`0.75rem` | 400  | footer, validation messages |

Line-height: ~1.1–1.3 for large headings, ~1.5–1.6 for body and descriptions.

---

## Spacing

Spacing is expressed in `rem`, in **quarter-rem (0.25rem / 4px) increments**.
Common step values in use: `0.25`, `0.375`, `0.5`, `0.625`, `1`, `1.125`,
`1.25`, `1.5`, `1.75`, `2`, `2.5`, `3rem`. Fixed-height controls use `px`
(`44px` inputs, `46px` submit, `72px` logo box).

- Reuse an existing step rather than inventing a new one.
- Keep vertical rhythm consistent between sibling components (e.g. every
  `.login-field` has the same `margin-bottom`).
- The strict "8px grid only" rule from earlier versions of this file does
  **not** apply; quarter-rem steps are the real unit.

---

## Border Radius

| Element                              | Radius   |
|--------------------------------------|----------|
| Large cards, brand logo box          | `16px`   |
| Inputs, primary buttons              | `8px`    |
| Feature boxes / secondary panels     | `12px`   |
| Small icon buttons / toggles         | `6px`    |

Use the same radius for elements of the same kind.

---

## Elevation (shadows)

Shadows **are used** in this project for elevated surfaces — the earlier
"NO SHADOWS" rule no longer applies. Keep them soft and blue-neutral.

| Use                     | Shadow                                                                 |
|-------------------------|-----------------------------------------------------------------------|
| Elevated card           | `0 20px 25px -5px rgba(0,0,0,.1), 0 8px 10px -6px rgba(0,0,0,.1)`      |
| Floating badge / logo   | `0 10px 30px rgba(0,0,0,.25)`                                          |
| Primary button (rest)   | `0 10px 15px -3px rgba(44,62,80,.2)`                                   |
| Primary button (hover)  | `0 14px 20px -3px rgba(44,62,80,.28)`                                  |
| Focus ring              | `0 0 0 3px rgba(91,159,189,.2)` (accent at low alpha)                  |

Do not put shadows on inputs at rest, hairline dividers, or body-level elements.

---

## Component Standards

Class naming follows a **BEM-ish** convention scoped by page/feature prefix:
`block`, `block__element`, `block--modifier`, plus state classes like
`is-visible`. Example from the login screen: `login-card`,
`login-card__body`, `login-input__field`, `login-input__toggle.is-visible`.

### Primary button
```css
.login-submit {
  width: 100%;
  height: 46px;
  border: 0;
  border-radius: 8px;
  color: #fff;
  font-size: 0.9375rem;
  font-weight: 700;
  cursor: pointer;
  background: linear-gradient(to right, var(--color-blue-accent) 0%, var(--color-blue-accent-dark) 100%);
  box-shadow: 0 10px 15px -3px rgba(44, 62, 80, 0.2);
  transition: transform 0.2s ease, box-shadow 0.2s ease, filter 0.2s ease;
}
.login-submit:hover  { filter: brightness(0.96); transform: scale(1.01); }
.login-submit:active { transform: scale(0.99); }
```
Global Bootstrap buttons are themed in `site.css` via `.btn-primary`
(solid `--color-blue-accent`, hover `--color-blue-accent-dark`).

### Card
```css
.login-card {
  width: 100%;
  max-width: 420px;
  background: #fff;
  border: 1px solid rgba(226, 232, 240, 0.7);
  border-radius: 16px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}
```

### Form input (icon-prefixed)
```css
.login-input__field {
  width: 100%;
  height: 44px;
  padding: 10px 12px 10px 40px;      /* left room for the leading icon */
  font-size: 0.875rem;
  color: var(--color-blue-dark);
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}
.login-input__field:focus {
  border-color: var(--color-blue-accent);
  box-shadow: 0 0 0 3px rgba(91, 159, 189, 0.2);
}
```
Inputs sit in a `.login-input` flex wrapper that positions an
absolute `.login-input__icon` on the left and an optional
`.login-input__toggle` (password show/hide) on the right.

### Validation / errors
- `<span asp-validation-for="...">` gets `.login-error` — `0.8125rem`, `#dc2626`.
- `<div asp-validation-summary="ModelOnly">` gets `.login-alert` — hidden when
  `:empty`, `role="alert"`.

---

## Layout Rules

- Full-screen auth pages use a **split layout**: `.login-page` is a flex row;
  a brand panel (`flex: 0 0 60%`) on the left and a centered form panel on the
  right.
- The brand panel is **hidden below `992px`** (`display: none`, shown again in
  `@media (min-width: 992px)`); the form panel is always full-width-friendly
  with a `max-width` card.
- Content cards cap at a `max-width` (login card: `420px`) and center with
  flexbox on the panel.
- Use flexbox / grid for layout; mobile-first.

### Breakpoints (Bootstrap 5 scale — this project uses these, not 640/1024/1280)
- `768px`  — root font-size step (`site.css`)
- `992px`  — show/hide the brand panel; Bootstrap `lg`
- `991.98px` — max-width companion query when needed

---

## Adding a Styled Page

1. Create the Razor view under `Views/<Area>/<Name>.cshtml` and set its
   `Layout` (auth pages use `_LayoutAuth`).
2. If the page needs its own CSS, add `wwwroot/css/<Name>.css` and pull it in
   from the view — do **not** add it to the layout:
   ```cshtml
   @section Styles {
       <link rel="stylesheet" href="~/css/<Name>.css" asp-append-version="true" />
   }
   ```
3. In that CSS, prefix every class with the page/feature name (`<name>-...`),
   reference `var(--color-*)` tokens for brand colors, and reuse the radius,
   spacing, and shadow values documented above.
4. Any page-specific JS goes in `@section Scripts` after the validation
   partial (see `Login.cshtml`'s password-toggle handler).

---

## Common Mistakes to AVOID

- ❌ Adding Google Fonts / font CDNs / bundled font files — this project uses the system stack only.
- ❌ Referencing `styles/global.css` or a `/fonts/` folder — they do not exist; tokens are in `wwwroot/css/site.css`.
- ❌ Hardcoding brand blues (`#5B9FBD`, `#2C3E50`, …) instead of `var(--color-*)`.
- ❌ Sizing type/spacing in `px` — the root font-size changes at 768px, so use `rem`.
- ❌ Inventing new spacing values outside the quarter-rem steps already in use.
- ❌ Mixing radius values on the same kind of element (pick 6 / 8 / 12 / 16).
- ❌ Putting page CSS in `_LayoutAuth.cshtml` instead of the view's `@section Styles`.
- ❌ Dropping the `.login-alert:empty { display: none }` guard or the `role="alert"`.
- ❌ Adding gradients to small elements — they are for the brand panel and the primary CTA only.

---

## Quick Reference

| Element        | Color                              | Size / weight        | Radius | Notes                    |
|----------------|------------------------------------|----------------------|--------|--------------------------|
| Page bg        | `--color-white-dirty` / `#f0f2f5`  | —                    | —      | full-bleed auth screens  |
| Card           | `--color-white`                    | —                    | 16px   | soft double shadow       |
| Card title     | `--color-blue-dark`                | 1.75rem / 700        | —      |                          |
| Body text      | `--color-blue-dark`                | 1rem / 400           | —      | line-height ~1.6         |
| Secondary text | `#64748b` / `--color-gray-soft`    | 0.875–0.9375rem / 400| —      |                          |
| Label          | `#334155`                          | 0.875rem / 600       | —      |                          |
| Input          | text `--color-blue-dark`, bg `#f8fafc`, border `#e2e8f0` | 0.875rem / 400 | 8px | 44px tall, focus = accent ring |
| Primary button | accent→accent-dark gradient, `#fff`| 0.9375rem / 700      | 8px    | 46px tall, lift on hover |
| Link           | `--color-blue-accent`              | 0.875rem / 600       | —      | underline on hover       |
| Error text     | `#dc2626`                          | 0.8125rem / 400      | —      |                          |
| Footer         | `#94a3b8` on `#f8fafc`             | 0.75rem / 400        | —      | top hairline `#eef1f5`   |

---

## Notes

- The login logo is a placeholder inline SVG (see the comment in
  `Views/Account/Login.cshtml`); a real QCU mark will replace it later.
- The global brand mark styles (`.site-brand`, `.site-brand__mark`) live in
  `site.css` and are the pattern to follow for header branding on other pages.
