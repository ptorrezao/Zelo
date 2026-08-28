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
    // So o servidor le isto (nao "public") - o token do Unleash nunca pode
    // chegar ao browser. Ver server/api/feature-flags.get.ts.
    unleash: {
      url: 'http://localhost:4242',
      apiToken: '*:*.unleash-insecure-api-token',
      environment: 'development',
    },
    public: {
      // Onde vive cada modulo. Os valores abaixo servem o desenvolvimento,
      // com cada app na sua porta. Atras do gateway passam a ser os caminhos
      // do Caddyfile, via NUXT_PUBLIC_ZELO_AUTO=/auto/ e equivalentes.
      zelo: {
        shell: 'http://localhost:3000',
        auto: 'http://localhost:3001',
        inventory: 'http://localhost:3002',
      },
      // Base da Zelo.Api. Atras do gateway passa a ser "/api" (ver Caddyfile).
      apiBase: 'http://localhost:8080',
      // Vazio em dev (localhost) - cookie de auth fica limitado ao host
      // atual, correto quando cada app corre numa porta diferente. Em
      // producao (subdominios) tem de ser o dominio partilhado, ex:
      // ".hugetower.cloud", via NUXT_PUBLIC_COOKIE_DOMAIN - sem isto, o
      // login feito na shell fica invisivel para a auto/inventory.
      cookieDomain: '',
    },
  },
})
