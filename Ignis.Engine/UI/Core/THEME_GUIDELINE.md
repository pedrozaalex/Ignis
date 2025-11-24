This is a critical architectural decision. When building a UI framework, the goal is **Semantic Naming**. You want names that describe *how* the color is used, not what the color looks like.

Here is a minimal, battle-tested set of color tokens required for a robust theming system, grouped by their function.

### 1. Brand & Action
These define the identity of the app and the primary interaction points (buttons, active tabs, links).

* `Primary`: The main brand color. Used for primary buttons, active states, and key highlights.
* `On-Primary`: The color of text or icons that sit *on top* of the Primary color. (Crucial for accessibility, usually white or black).

### 2. Backgrounds & Surfaces
You need to distinguish between the general application background and specific content areas (cards, sidebars, modals).

* `Background`: The underlying color of the application window or page (usually very light gray or black in dark mode).
* `Surface`: The background color for elements that sit *on* the background (cards, modals, dropdowns).
* `Surface-Overlay`: (Optional but recommended) A slightly lighter/elevated shade for hover states or elements distinct from the standard Surface.

### 3. Content (Typography & Icons)
Never just use "Black" or "White." Using semantic opacity levels allows text to adapt gracefully between Light and Dark modes.

* `Text-Main`: The highest contrast text. Used for headings and body text (e.g., nearly black in light mode, white in dark mode).
* `Text-Muted`: Lower contrast text. Used for secondary information, placeholders, or helper text.
* `Text-On-Surface`: Explicitly defined for text sitting on `Surface` elements, if different from `Background`.

### 4. Structural Elements
These are essential for layout definitions but should not compete with the content.

* `Border`: Used for inputs, dividers, and card outlines.
* `Border-Focus`: A specific highlight color (often linked to `Primary`) for accessibility focus rings on inputs/buttons.

### 5. Functional (Feedback)
Every app needs to communicate state.

* `Success`: For completion messages, success toasts, or positive trends.
* `Error`: For destructive actions, form errors, or critical alerts.
* `Warning`: For non-critical alerts or "needs attention" states.

---

### Summary Table (The "Minimal" API)

If you were to write this as a TypeScript interface or CSS Variables, this is the tightest possible list that still allows for a fully functional, professional UI:

| Category | Token Name | Typical Usage |
| :--- | :--- | :--- |
| **Brand** | `primary` | Main buttons, links |
| | `on-primary` | Text inside main buttons |
| **Canvas** | `background` | The app `<body>` color |
| | `surface` | Cards, Modals, Sidebars |
| **Content** | `text-main` | Headings, Paragraphs |
| | `text-muted` | Captions, Placeholders |
| **UI** | `border` | Dividers, Input borders |
| **Status** | `success` | Validations, "Done" |
| | `error` | Deletions, "Failed" |

### A Note on "States" (Hover/Pressed)
To keep the set minimal, do **not** manually name every hover color (e.g., `Primary-Dark`, `Primary-Light`).

Instead, use an **opacity modifier** or a **brightness calculator** in your framework code.
* *Example:* The hover state of a button is simply the `Primary` color with a 90% brightness filter or a black overlay at 10% opacity. This saves you from managing double the amount of variables.
