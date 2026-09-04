# CLAUDE.md
# Minimalistic Design System with Local Fonts

## Project Structure
```
project-root/
├── CLAUDE.md
├── fonts/
│   ├── inter-regular.woff2
│   ├── inter-semibold.woff2
│   ├── inter-bold.woff2
│   ├── playfair-display-regular.woff2
│   └── playfair-display-bold.woff2
├── styles/
│   └── global.css (shared by all pages)
```

## Color Palette
**NEVER use Google Fonts or external font CDNs. Download fonts and store locally in /fonts/**

### Primary Colors
- **Light Blue:** #E8F0F8 (very light, almost off-white with blue tint)
- **Light Blue (accent):** #5B9FBD (muted, professional blue)
- **Dirty White:** #F5F3F0 (warm off-white, slightly gray undertone)
- **Deep Blue:** #2C3E50 (dark blue-gray for text and accents)

### Secondary Colors
- **Soft Gray:** #A0A0A0 (for secondary text, disabled states)
- **Light Gray:** #E0E0E0 (for borders and dividers)
- **White:** #FFFFFF (for overlays and special cases only)

### Text Colors
- **Primary Text:** #2C3E50 (deep blue-gray on light backgrounds)
- **Secondary Text:** #A0A0A0 (soft gray for captions, metadata)
- **Accent Text:** #5B9FBD (light blue for links, highlights)

### Usage Rules
- Background: Dirty White (#F5F3F0) or Light Blue (#E8F0F8)
- Cards/Sections: White (#FFFFFF) or Light Blue (#E8F0F8)
- Buttons: Light Blue accent (#5B9FBD) background
- Borders: Light Gray (#E0E0E0) — never use dark colors
- Text: Deep Blue (#2C3E50)

---

## Typography

### Font System (LOCAL ONLY - NO GOOGLE FONTS CDN)
- **Display/Headings:** Playfair Display (elegant, serif, distinctive)
  - File: playfair-display-regular.woff2, playfair-display-bold.woff2
- **Body/UI:** Inter (clean, readable, sans-serif)
  - Files: inter-regular.woff2, inter-semibold.woff2, inter-bold.woff2

### Font Sizes & Weights
- **h1 (Hero/Page Title):** Playfair Display Bold, 56px / 3.5rem, line-height 1.2
- **h2 (Section Title):** Playfair Display Bold, 40px / 2.5rem, line-height 1.3
- **h3 (Subsection):** Playfair Display Regular, 32px / 2rem, line-height 1.3
- **Body Text:** Inter Regular, 16px / 1rem, line-height 1.6
- **Small Text/Caption:** Inter Regular, 14px / 0.875rem, line-height 1.5
- **Button Text:** Inter Semibold, 16px / 1rem
- **Navigation:** Inter Semibold, 16px / 1rem

### Font Weight Rules
- Use only: 400 (Regular), 600 (Semibold), 700 (Bold)
- Never use 300 or 500 — too thin or awkward

---

## Spacing System (8px Grid)
- **xs:** 4px (0.25rem) — for tiny gaps
- **sm:** 8px (0.5rem) — button padding
- **md:** 16px (1rem) — standard spacing
- **lg:** 24px (1.5rem) — section padding, card padding
- **xl:** 32px (2rem) — between major sections
- **2xl:** 48px (3rem) — large gaps, hero sections
- **3xl:** 64px (4rem) — very large section breaks

Use these consistently in padding, margins, gaps. **NEVER use random values like 15px, 22px, etc.**

---

## Component Standards

### Buttons
```css
.button {
  background-color: var(--color-blue-accent);
  color: #FFFFFF;
  padding: 12px 24px; /* sm lg */
  border: none;
  border-radius: 6px;
  font-family: var(--font-body);
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.button:hover {
  background-color: #4A7FA0; /* slightly darker */
}

.button--secondary {
  background-color: var(--color-gray-light);
  color: var(--color-text-primary);
}
```

### Cards
```css
.card {
  background-color: #FFFFFF;
  border: 1px solid var(--color-gray-border);
  border-radius: 8px;
  padding: 24px; /* lg */
  box-shadow: none; /* NO SHADOWS */
}
```

### Navigation
```css
nav {
  background-color: var(--color-white-dirty);
  border-bottom: 1px solid var(--color-gray-border);
  padding: 16px 24px; /* md lg */
  position: sticky;
  top: 0;
  z-index: 100;
}

nav a {
  color: var(--color-text-primary);
  text-decoration: none;
  font-weight: 600;
  font-size: 1rem;
}

nav a:hover {
  color: var(--color-blue-accent);
}
```

### Form Inputs
```css
input, textarea, select {
  border: 1px solid var(--color-gray-border);
  border-radius: 6px;
  padding: 12px 16px;
  font-family: var(--font-body);
  font-size: 1rem;
  background-color: #FFFFFF;
  color: var(--color-text-primary);
}

input:focus, textarea:focus {
  outline: none;
  border-color: var(--color-blue-accent);
  box-shadow: 0 0 0 3px rgba(91, 159, 189, 0.1);
}
```

### Sections
```css
section {
  padding: 48px 24px; /* 3xl lg */
  background-color: var(--color-white-dirty);
}

section.alt {
  background-color: var(--color-blue-light);
}
```

---

## Design Rules (CRITICAL - FOLLOW EVERY PAGE)

### Color Rules
- ✅ **DO:** Use CSS variables for all colors (defined in global.css :root)
- ❌ **DON'T:** Hardcode hex values like `#5B9FBD` in component files
- ✅ **DO:** Stick to the 5-color palette (light blue, dirty white, deep blue, soft gray, light gray)
- ❌ **DON'T:** Add new colors or gradients

### Font Rules
- ✅ **DO:** Use Playfair Display ONLY for h1, h2, h3 (headings)
- ✅ **DO:** Use Inter ONLY for body, buttons, navigation
- ❌ **DON'T:** Mix fonts within the same page
- ❌ **DON'T:** Use system fonts or fallbacks — fonts are stored locally in /fonts/
- ✅ **DO:** Load fonts once in global.css using @font-face with local files

### Spacing Rules
- ✅ **DO:** Use multiples of 8px (4, 8, 16, 24, 32, 48, 64)
- ❌ **DON'T:** Use arbitrary values like 15px, 22px, 35px
- ✅ **DO:** Use consistent padding/margin across all cards and sections
- ❌ **DON'T:** Add different spacing to similar components

### Shadow Rules
- ✅ **DO:** Use subtle, minimal shadows if needed: `box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);`
- ❌ **DON'T:** Use heavy drop shadows or dark shadows
- ✅ **DO:** Use borders (1px light gray) instead of shadows for definition

### Border Radius Rules
- **Cards & buttons:** 6px–8px
- **Inputs:** 6px
- **Large sections:** 0px (square corners) or 12px (for prominent cards)
- ❌ **DON'T:** Use different border-radius values for similar components

### Layout Rules
- ✅ **DO:** Use max-width: 1200px for content on large screens
- ✅ **DO:** Center content with margin: 0 auto
- ✅ **DO:** Use CSS Grid or Flexbox for layouts
- ✅ **DO:** Implement mobile-first responsive design
- Breakpoints: 640px (sm), 1024px (md), 1280px (lg)

### Dark Mode (Optional but Recommended)
- If implementing dark mode, use CSS media query: `@media (prefers-color-scheme: dark)`
- Swap background colors but keep the same accent colors

---

## Global CSS Structure (styles/global.css)

**Every page MUST import this file:**
```html
<link rel="stylesheet" href="../styles/global.css">
```

The file should include:
1. @font-face declarations for Playfair Display and Inter (from /fonts/)
2. CSS custom properties (:root) with all colors and spacing variables
3. Base styles for HTML elements (body, h1-h6, p, a, button, form elements)
4. Utility classes (.text-center, .text-small, .flex, .grid, etc.)
5. Common component styles (.card, .button, .section, nav, etc.)
6. Responsive utilities for mobile/tablet/desktop
7. Print styles (optional)

**IMPORTANT:** global.css is shared by ALL pages. Changes here affect the entire site.

---

## Font Loading (LOCAL FILES - CRITICAL)

In global.css, load fonts from the /fonts/ directory:

```css
@font-face {
  font-family: 'Playfair Display';
  src: url('../fonts/playfair-display-regular.woff2') format('woff2');
  font-weight: 400;
  font-display: swap;
}

@font-face {
  font-family: 'Playfair Display';
  src: url('../fonts/playfair-display-bold.woff2') format('woff2');
  font-weight: 700;
  font-display: swap;
}

@font-face {
  font-family: 'Inter';
  src: url('../fonts/inter-regular.woff2') format('woff2');
  font-weight: 400;
  font-display: swap;
}

@font-face {
  font-family: 'Inter';
  src: url('../fonts/inter-semibold.woff2') format('woff2');
  font-weight: 600;
  font-display: swap;
}

@font-face {
  font-family: 'Inter';
  src: url('../fonts/inter-bold.woff2') format('woff2');
  font-weight: 700;
  font-display: swap;
}
```

---

## CSS Variables (in :root in global.css)

```css
:root {
  /* Colors */
  --color-blue-light: #E8F0F8;
  --color-blue-accent: #5B9FBD;
  --color-blue-dark: #2C3E50;
  --color-white-dirty: #F5F3F0;
  --color-gray-soft: #A0A0A0;
  --color-gray-border: #E0E0E0;
  --color-white: #FFFFFF;

  /* Fonts */
  --font-display: 'Playfair Display', serif;
  --font-body: 'Inter', sans-serif;

  /* Spacing */
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 2rem;
  --spacing-2xl: 3rem;
  --spacing-3xl: 4rem;

  /* Typography */
  --font-size-xs: 0.875rem;
  --font-size-sm: 1rem;
  --font-size-md: 1.25rem;
  --font-size-lg: 2rem;
  --font-size-xl: 2.5rem;
  --font-size-2xl: 3.5rem;

  /* Border Radius */
  --radius-sm: 4px;
  --radius-md: 6px;
  --radius-lg: 8px;
  --radius-xl: 12px;
}
```

---

## Common Mistakes to AVOID

- ❌ **Hardcoded hex colors** — Use CSS variables ONLY
- ❌ **Google Fonts CDN** — Download fonts and store in /fonts/ locally
- ❌ **Different font families per page** — Always Playfair + Inter
- ❌ **Random spacing values** — Use 8px multiples ONLY
- ❌ **Drop shadows** — Use borders and subtle shadows only
- ❌ **Multiple button styles** — One .button class for consistency
- ❌ **Different card designs** — One .card style for all cards
- ❌ **Inconsistent navigation** — Same nav style on every page
- ❌ **Not importing global.css** — Every page MUST link it
- ❌ **New colors not in palette** — Stick to the 5 core colors

---

## File Checklist for Every Page

Before submitting any page, verify:
- ✅ global.css is imported in `<head>`
- ✅ All colors use CSS variables
- ✅ Only Playfair Display for headings
- ✅ Only Inter for body and UI
- ✅ Spacing is 8px multiples
- ✅ No hardcoded hex values
- ✅ Navigation is consistent
- ✅ Buttons use .button class
- ✅ Cards use .card class
- ✅ Forms have proper labels and focus states
- ✅ Mobile responsive (tested at 640px, 1024px, 1280px)
- ✅ No external font CDNs

---

## Example: How to Create a New Page

1. Create `/pages/newpage.html`
2. Copy this template:

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Page Title</title>
  <link rel="stylesheet" href="../styles/global.css">
</head>
<body>
  <nav>
    <a href="index.html">Home</a>
    <a href="about.html">About</a>
  </nav>

  <main>
    <section>
      <h1>Page Heading</h1>
      <p>Body text goes here...</p>
    </section>

    <section class="alt">
      <h2>Section Title</h2>
      <div class="card">
        <p>Card content</p>
      </div>
    </section>

    <section>
      <button class="button">Call to Action</button>
    </section>
  </main>

  <footer>
    <p>&copy; 2024. All rights reserved.</p>
  </footer>
</body>
</html>
```

---

## Design Inspiration & References

- **Aesthetic:** Minimalistic, professional, corporate-tech
- **Feel:** Clean, calm, trustworthy (light blue + dirty white creates a serene, professional vibe)
- **Inspiration Sites:** Stripe.com, Linear.app, Notion.so (minimal, elegant, professional)
- **Typography Pairing:** Playfair Display (editorial elegance) + Inter (modern, clean)

---

## Quick Reference

| Element | Color | Font | Size | Spacing |
|---------|-------|------|------|---------|
| h1 | Deep Blue | Playfair Bold | 3.5rem | 48px top/bottom |
| h2 | Deep Blue | Playfair Bold | 2.5rem | 32px top/bottom |
| Body | Deep Blue | Inter Regular | 1rem | 16px line-height |
| Link | Light Blue Accent | Inter Regular | 1rem | — |
| Button | Light Blue Bg / White Text | Inter Semibold | 1rem | 12px 24px |
| Card | White / Light Blue Border | — | — | 24px padding |
| Section | Dirty White or Light Blue | — | — | 48px padding |
| Navigation | Dirty White | Inter Semibold | 1rem | 16px 24px |

---

## Support

If Claude ever breaks these rules:
- Type: `@CLAUDE.md Please follow the design system in CLAUDE.md`
- Or: `Review CLAUDE.md and rebuild this page with proper colors, fonts, and spacing`

This ensures consistency across your entire website, every page, every time.