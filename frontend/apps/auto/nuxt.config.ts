export default defineNuxtConfig({
  extends: ['@zelo/ui'],
  devtools: { enabled: false },
  devServer: { port: 3001 },

  app: {
    // Cada app fica no seu proprio subdominio (sem gateway/path-rewrite
    // à frente) - fica '/' em todos os ambientes. NUXT_APP_BASE_URL so
    // continua configuravel para nao partir este caminho se um dia
    // voltarmos a um gateway com caminhos partilhados.
    baseURL: process.env.NUXT_APP_BASE_URL ?? '/',
  },
})
