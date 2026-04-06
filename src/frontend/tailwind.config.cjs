module.exports = {
  // Use the `.dark` class to toggle dark mode (matches your PrimeVue config)
  darkMode: "class",
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx,html}",
  ],
  theme: {
    container: {
      center: true,
      padding: {
        DEFAULT: "1rem",
        sm: "1rem",
        lg: "2rem",
        xl: "4rem",
      },
    },
    extend: {
      // Lightweight sensible defaults - you can customize this later if you want a design system
      fontFamily: {
        sans: ["Inter", "ui-sans-serif", "system-ui", "Segoe UI", "Helvetica Neue", "Arial"],
      },
      colors: {
        // Primary color to align with a modern blue accent; keep a few steps for utility usage
        primary: {
          50: "#f5f9ff",
          100: "#eaf3ff",
          200: "#cfe6ff",
          300: "#9fceff",
          400: "#66b0ff",
          500: "#2b8bff",
          600: "#1f6fe6",
          700: "#1754b4",
          800: "#113a82",
          900: "#0b2758",
        },
      },
      boxShadow: {
        subtle: "0 1px 3px rgba(15, 23, 42, 0.06), 0 1px 2px rgba(15, 23, 42, 0.04)",
      },
      borderRadius: {
        xl: "1rem",
      },
      transitionProperty: {
        height: "height",
        spacing: "margin, padding",
      },
    },
  },
  plugins: [
    // No extra Tailwind plugins specified by default so this file remains minimal.
    // If you add `@tailwindcss/forms` or other official plugins to devDependencies,
    // you can enable them here, e.g.:
    // require('@tailwindcss/forms'),
  ],
  // Optionally preserve utility classes produced dynamically by PrimeVue templates or runtime code.
  // If you find utilities are being purged that are generated dynamically (e.g., `text-${color}`),
  // add safelist patterns here.
  safelist: [
    // Example: keep possible dynamic background/text utilities (uncomment/edit if needed)
    // { pattern: /bg-(primary|red|green|blue)-(50|100|200|300|400|500|600|700|800|900)/ },
    // { pattern: /text-(primary|red|green|blue)-(50|100|200|300|400|500|600|700|800|900)/ }
  ],
};
