# Frontend

A clean, barebones Vue 3 + TypeScript frontend with Tailwind CSS and PrimeVue.

## Tech Stack

- **Vue 3** - Progressive JavaScript framework
- **TypeScript** - Type safety
- **Vite** - Fast build tool
- **Tailwind CSS v4** - Utility-first CSS framework
- **PrimeVue v4** - Rich UI component library
- **Bun** - Fast JavaScript runtime and package manager

## Getting Started

### Install Dependencies

```bash
bun install
```

### Development

```bash
bun run dev
```

The dev server will start at `http://localhost:5173/`

### Build for Production

```bash
bun run build
```

Output will be in the `dist/` directory.

### Preview Production Build

```bash
bun run preview
```

## Project Structure

```
src/frontend/
├── src/
│   ├── components/       # Vue components
│   ├── App.vue          # Root component
│   ├── main.ts          # App entry point
│   └── style.css        # Global styles (Tailwind)
├── index.html           # HTML entry point
├── vite.config.ts       # Vite configuration
└── package.json         # Dependencies and scripts
```

## Using PrimeVue Components

PrimeVue components can be imported and used directly:

```vue
<script setup lang="ts">
import Button from "primevue/button";
import Card from "primevue/card";
</script>

<template>
  <Card>
    <template #title>Hello</template>
    <template #content>
      <Button label="Click me" />
    </template>
  </Card>
</template>
```

## Using Tailwind CSS

Tailwind utility classes work alongside PrimeVue:

```vue
<template>
  <div class="flex items-center gap-4 p-6 bg-gray-100">
    <Button label="Submit" class="w-full" />
  </div>
</template>
```

## Dark Mode

Dark mode is configured and can be toggled by adding the `dark` class to the root element or any parent element.

## Resources

- [Vue 3 Documentation](https://vuejs.org/)
- [Tailwind CSS Documentation](https://tailwindcss.com/)
- [PrimeVue Documentation](https://primevue.org/)
- [Vite Documentation](https://vite.dev/)