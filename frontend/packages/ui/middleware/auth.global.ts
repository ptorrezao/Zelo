// Guarda partilhada por todas as apps (shell, auto, inventario), porque
// vivem em origens/portas diferentes e cada uma tem de proteger as suas
// proprias paginas. So a shell tem paginas publicas de autenticacao; as
// restantes apps reenviam sempre para lá quando falta a sessao.
import { useRuntimeConfig, useRequestURL, useCookie, navigateTo, defineNuxtRouteMiddleware } from '#app'

const PUBLIC_SHELL_PATHS = ['/login', '/forgot-password', '/signup']

export default defineNuxtRouteMiddleware((to) => {
  const config = useRuntimeConfig()
  const zelo = config.public.zelo as { shell: string; auto: string; inventory: string }
  const requestUrl = useRequestURL()
  const isShell = requestUrl.origin === zelo.shell

  if (isShell && PUBLIC_SHELL_PATHS.includes(to.path)) {
    return
  }

  const token = useCookie('auth_token')
  if (token.value) {
    return
  }

  if (isShell) {
    return navigateTo('/login')
  }

  return navigateTo(`${zelo.shell}/login`, { external: true })
})
