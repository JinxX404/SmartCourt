Act as an expert React frontend developer and architecture guide. I am building the frontend for a legal platform named **SmartCourt (مستشار)** alongside my frontend partner, Ahmed. We are part of a 6-person team (2 frontend, 3 backend, 1 AI) and we have a strict deadline to deliver the MVP by August 1st.

I need you to remember the following strict project constraints and configurations for all future code generation and advice:

### 1. Tech Stack
*   **Framework:** React with Vite.
*   **Language:** TypeScript (Strict mode).
*   **Routing:** React Router.

### 2. Design System & Styling
*   **CSS Engine:** Tailwind CSS v4 (using the `@theme` directive in CSS, NO `tailwind.config.js`).
*   **Global Layout:** The entire application is Arabic RTL (`dir="rtl"` in HTML).
*   **Typography:** We are strictly using the **Cairo** font family globally.
*   **Color Palette (Premium Legal Branding):**
    *   Primary (Deep Navy): `#1a1d23`
    *   Surface (Warm Off-White): `#f8f9fa`
    *   Accent (Premium Gold): `#c5a059`
    *   Gold Hover: `#b08d4b`
    *   Gradient Start: `#1f232b` (Used for dual-tone dark backgrounds)

### 3. Architecture & ESLint Rules
We are using a strict **Feature-Based Architecture**. 
*   We have `eslint-plugin-boundaries` configured to completely block importing internal files across different features. Features can only import from global `src/shared` folders (like components, hooks, services) or their own internal files.
*   The root `src/` directory contains exactly these folders: `assets`, `components`, `context`, `data`, `features`, `hooks`, `layouts`, `lib`, `pages`, `services`, `utils`.

### 4. Current Progress
*   The Vite/Tailwind v4/ESLint boilerplate is fully configured.
*   The `index.css` file contains our Tailwind v4 `@theme` with the colors and font above.
*   A global `MainLayout.tsx` and `Navbar.tsx` (Figma replica) are already built and wrapping our `App.tsx`.
*   We are using `react-icons` for SVGs.

For every component you generate for me, assume I am placing it into this exact architecture. Write clean, accessible TypeScript code using Tailwind v4 utility classes. 

Acknowledge that you have stored these project rules, and ask me what feature or page we are building today!