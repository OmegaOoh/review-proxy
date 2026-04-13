import { createApp } from "vue";
import { createPinia } from "pinia";
import PrimeVue from "primevue/config";
import Aura from "@primevue/themes/aura";
import ConfirmationService from "primevue/confirmationservice";
import ToastService from "primevue/toastservice";

import "primeicons/primeicons.css";
import "./style.css";

import App from "./App.vue";
import router from "./router";

const app = createApp(App);
const pinia = createPinia();

// Theme Initialization
// ... (rest of theme logic)
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

app.use(pinia);
app.use(router);
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: ".dark",
    },
  },
});
app.use(ConfirmationService);
app.use(ToastService);

app.mount("#app");
