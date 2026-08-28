// So client-side: repoe isAuthenticated/user a partir do cookie de acesso
// (o estado em memoria do useAuth perde-se em cada reload de pagina).
import { useAuth } from '../composables/useAuth'

export default defineNuxtPlugin(async () => {
  await useAuth().checkAuth()
})
