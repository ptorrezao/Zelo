// Layer partilhado pelas apps. Cada app faz extends: ['@zelo/ui'],
// o que lhe da os componentes por auto-import, o layout e os tokens.
import { fileURLToPath } from 'node:url'
import tailwindcss from '@tailwindcss/vite'

export default defineNuxtConfig({
  alias: {
    '@': '.',
  },
  css: [fileURLToPath(new URL('./styles/global.css', import.meta.url))],
  vite: {
    plugins: [tailwindcss()],
  },

  runtimeConfig: {
    public: {
      // Onde vive cada modulo. Os valores abaixo servem o desenvolvimento,
      // com cada app na sua porta. Atras do gateway passam a ser os caminhos
      // do Caddyfile, via NUXT_PUBLIC_ZELO_AUTO=/auto/ e equivalentes.
      zelo: {
        shell: 'http://localhost:3000',
        auto: 'http://localhost:3001',
        inventory: 'http://localhost:3002',
      },
    },
  },
})
