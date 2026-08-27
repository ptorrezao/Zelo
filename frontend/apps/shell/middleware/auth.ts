export default defineRouteMiddleware((to, from) => {
  const token = useCookie('auth_token')

  // Allow access to login page without authentication
  if (to.path === '/login') {
    return
  }

  // Check if authenticated
  if (!token.value) {
    return navigateTo('/login')
  }
})
