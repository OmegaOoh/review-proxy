import { createApp } from "vue";
import { createPinia } from "pinia";
import PrimeVue from "primevue/config";
import Aura from "@primevue/themes/aura";
import { definePreset } from "@primevue/themes";
import ConfirmationService from "primevue/confirmationservice";
import ToastService from "primevue/toastservice";

import "primeicons/primeicons.css";
import "./style.css";

import App from "./App.vue";
import router from "./router";

// Theme Initialization (must happen before App instantiation to prevent flicker and sync issues)
const initTheme = () => {
  try {
    const prefersDark = window.matchMedia(
      "(prefers-color-scheme: dark)",
    ).matches;
    const storedTheme = localStorage.getItem("theme");
    const useDark = storedTheme ? storedTheme === "dark" : prefersDark;

    if (useDark) {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }
  } catch (e) {
    console.warn("Theme initialization failed", e);
  }
};

initTheme();

const MyPreset = definePreset(Aura, {
  semantic: {
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
      950: "#06183a",
    },
  },
});

const app = createApp(App);
const pinia = createPinia();

app.use(pinia);
app.use(router);
app.use(PrimeVue, {
  theme: {
    preset: MyPreset,
    options: {
      darkModeSelector: ".dark",
    },
  },
});
app.use(ConfirmationService);
app.use(ToastService);

app.mount("#app");
