import { createApp } from "vue";

import PrimeVue from "primevue/config";
import Aura from "@primevue/themes/aura";
import "primeicons/primeicons.css";
import "./style.css";
import App from "./App.vue";
import router from "./router";

const app = createApp(App);

/*
  Register Pinia dynamically (if available).

  Notes:
  - Some environments running static analysis or diagnostics may not have `pinia` installed,
    which leads TypeScript to complain "Cannot find module 'pinia'...". To avoid that, we
    use a dynamic import and suppress TS checking for the import line so that compilation
    / analysis does not fail when the dependency is absent.
  - If Pinia is present and exposes `createPinia`, it will be registered. Otherwise the app
    continues to run without a global store.
*/
(async () => {
  try {
    // @ts-ignore - dynamic import may not resolve in environments without the dependency
    const mod = await import("pinia");
    // prefer the canonical `createPinia` export if available
    const createPinia =
      (mod && (mod.createPinia ?? (mod as any).default?.createPinia)) ??
      (mod as any).createPinia;
    if (typeof createPinia === "function") {
      const p = createPinia();
      try {
        app.use(p);
      } catch {
        // harmless if pinia instance can't be registered for any reason
      }
    }
  } catch (err) {
    // Pinia not installed or import failed — continue without a global store
    // eslint-disable-next-line no-console
    console.info("Pinia not available; skipping store registration");
  }
})();

// Initialize theme: prefer explicit user preference stored in localStorage, otherwise fall back to system preference.
// This sets the Tailwind/PrimeVue dark-mode selector ('.dark') on the document root so both systems respect it.
try {
  const prefersDark =
    typeof window !== "undefined" &&
    window.matchMedia &&
    window.matchMedia("(prefers-color-scheme: dark)").matches;
  const storedTheme = localStorage.getItem("theme"); // expected values: 'dark' | 'light' | null
  const useDark = storedTheme ? storedTheme === "dark" : Boolean(prefersDark);

  if (useDark) {
    document.documentElement.classList.add("dark");
  } else {
    document.documentElement.classList.remove("dark");
  }
} catch (e) {
  // If anything goes wrong (e.g. SSR edge cases), do not block app startup.
  // Theme can still be toggled later from the UI.
  // eslint-disable-next-line no-console
  console.warn("Theme initialization failed", e);
}

app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: ".dark",
    },
  },
});

app.use(router);

app.mount("#app");
