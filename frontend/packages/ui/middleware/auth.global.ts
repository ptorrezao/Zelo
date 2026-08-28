// Guarda partilhada por todas as apps (shell, auto, inventario), porque
// vivem em origens/portas diferentes e cada uma tem de proteger as suas
// proprias paginas. So a shell tem paginas publicas de autenticacao; as
// restantes apps reenviam sempre para lá quando falta a sessao.
import { useRuntimeConfig, useRequestURL, useCookie, navigateTo, defineNuxtRouteMiddleware } from '#app'
import { ACCESS_TOKEN_COOKIE } from '../composables/useApiClient'
import { useFeatureFlags } from '../composables/useFeatureFlags'

const PUBLIC_SHELL_PATHS = ['/login', '/forgot-password', '/signup']

export default defineNuxtRouteMiddleware(async (to) => {
  const config = useRuntimeConfig()
  const zelo = config.public.zelo as { shell: string; auto: string; inventory: string }
  const requestUrl = useRequestURL()
  const isShell = requestUrl.origin === zelo.shell

  // A app inteira fica desligada quando a flag esta off - nao basta
  // esconder o link na sidebar da shell, quem entrar por URL direta (ou
  // ja tinha a app aberta) tem de ser mandado embora tambem.
  if (!isShell) {
    const flagKey = requestUrl.origin === zelo.auto
      ? 'autoAppEnabled'
      : requestUrl.origin === zelo.inventory
        ? 'inventoryAppEnabled'
        : null

    if (flagKey) {
      const { data: flags } = await useFeatureFlags()
      if (flags.value[flagKey] === false) {
        return navigateTo(zelo.shell, { external: true })
      }
    }
  }

  if (isShell && PUBLIC_SHELL_PATHS.includes(to.path)) {
    return
  }

  const token = useCookie(ACCESS_TOKEN_COOKIE)
  if (token.value) {
    return
  }

  if (isShell) {
    return navigateTo('/login')
  }

  return navigateTo(`${zelo.shell}/login`, { external: true })
})
