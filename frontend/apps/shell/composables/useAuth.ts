import { ref, computed } from 'vue'
import { useCookie } from '#app'
import { ACCESS_TOKEN_COOKIE, REFRESH_TOKEN_COOKIE, useApiClient } from '@zelo/ui/composables/useApiClient'
import { describeIdentityError } from '../utils/identityErrors'

const isAuthenticated = ref(false)
const user = ref<{ email: string } | null>(null)

export function useAuth() {
  const client = useApiClient()

  const login = async (email: string, password: string) => {
    const { data, error } = await client.POST('/api/auth/login', {
      params: { query: { useCookies: false } },
      body: { email, password },
    })

    if (error || !data) {
      throw new Error('Email ou palavra-passe inválidos')
    }

    useCookie(ACCESS_TOKEN_COOKIE, { maxAge: Number(data.expiresIn) }).value = data.accessToken
    useCookie(REFRESH_TOKEN_COOKIE).value = data.refreshToken

    isAuthenticated.value = true
    user.value = { email }
  }

  const register = async (email: string, password: string) => {
    const { error } = await client.POST('/api/auth/register', {
      body: { email, password },
    })

    if (error) {
      throw new Error(describeIdentityError(error, 'Não foi possível criar a conta.'))
    }
    // Conta criada mas por confirmar - RequireConfirmedEmail esta ativo no
    // backend, o login so funciona depois de o utilizador clicar no link
    // que o Mailhog/SMTP entregou. Nao faz sentido tentar login aqui.
  }

  const forgotPassword = async (email: string) => {
    await client.POST('/api/auth/forgotPassword', { body: { email } })
    // O endpoint responde 200 quer o email exista quer nao (evita
    // confirmar a terceiros que endereco esta registado) - nunca rejeitar
    // com base na resposta.
  }

  const resetPassword = async (email: string, resetCode: string, newPassword: string) => {
    const { error } = await client.POST('/api/auth/resetPassword', {
      body: { email, resetCode, newPassword },
    })

    if (error) {
      throw new Error(describeIdentityError(error, 'Não foi possível repor a palavra-passe.'))
    }
  }

  const logout = () => {
    isAuthenticated.value = false
    user.value = null

    useCookie(ACCESS_TOKEN_COOKIE).value = null
    useCookie(REFRESH_TOKEN_COOKIE).value = null
  }

  /// Repoe isAuthenticated/user a partir do cookie apos um reload - o
  /// estado em memoria (os refs acima) perde-se sempre que o modulo JS
  /// recarrega, o cookie e o unico estado que sobrevive.
  const checkAuth = async () => {
    const token = useCookie(ACCESS_TOKEN_COOKIE)
    if (!token.value) {
      isAuthenticated.value = false
      user.value = null
      return
    }

    const { data, error } = await client.GET('/api/auth/manage/info')
    if (error || !data) {
      isAuthenticated.value = false
      user.value = null
      return
    }

    isAuthenticated.value = true
    user.value = { email: data.email }
  }

  return {
    isAuthenticated: computed(() => isAuthenticated.value),
    user: computed(() => user.value),
    login,
    register,
    forgotPassword,
    resetPassword,
    logout,
    checkAuth,
  }
}
