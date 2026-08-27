export default defineNuxtConfig({
  extends: ['@zelo/ui'],
  devtools: { enabled: false },
  devServer: { port: 3002 },

  app: {
    // Ver a nota em apps/auto: aqui o prefixo do gateway e /inventario/.
    baseURL: process.env.NUXT_APP_BASE_URL ?? '/',
  },
})
