import { ref, computed } from 'vue'

const isAuthenticated = ref(false)
const user = ref<{ email: string } | null>(null)

export function useAuth() {
  const login = (email: string, password: string) => {
    // TODO: Replace with actual API call
    isAuthenticated.value = true
    user.value = { email }

    // Use cookie for auth token (survives page reload and is accessible to middleware)
    const token = useCookie('auth_token')
    token.value = 'mock_token_' + Date.now()
  }

  const logout = () => {
    isAuthenticated.value = false
    user.value = null

    const token = useCookie('auth_token')
    token.value = null
  }

  const checkAuth = () => {
    const token = useCookie('auth_token')
    isAuthenticated.value = !!token.value
  }

  return {
    isAuthenticated: computed(() => isAuthenticated.value),
    user: computed(() => user.value),
    login,
    logout,
    checkAuth,
  }
}
