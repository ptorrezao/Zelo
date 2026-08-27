export default defineNuxtConfig({
  extends: ['@zelo/ui'],
  devtools: { enabled: false },
  devServer: { port: 3001 },

  app: {
    // O Caddyfile encaminha /auto/* sem remover o prefixo, portanto em
    // producao esta app corre com NUXT_APP_BASE_URL=/auto/ — sem isso os
    // assets sao pedidos na raiz e o gateway entrega-os ao shell.
    baseURL: process.env.NUXT_APP_BASE_URL ?? '/',
  },
})
