import { defineConfig } from 'astro/config';
import react from '@astrojs/react';
import tailwind from '@astrojs/tailwind';


export default defineConfig({
  i18n: {
    defaultLocale: 'en',
    locales: ['es', 'en'],
    routing:{
      prefixDefaultLocale: false,
    }
  },
  site: 'https://arturh85.github.io/',
  base: '/AnkiTaiso',
  output: 'static',
  integrations: [tailwind(), react()],
});
