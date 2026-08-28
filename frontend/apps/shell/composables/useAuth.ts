import { ref, computed } from 'vue'
import { useCookie } from '#app'
import { ACCESS_TOKEN_COOKIE, REFRESH_TOKEN_COOKIE, useApiClient } from '@zelo/ui/composables/useApiClient'
import type { components } from '@zelo/api-client'

const isAuthenticated = ref(false)
const user = ref<{ email: string } | null>(null)

// Os codigos de erro do ASP.NET Identity vem em ingles (errors do
// HttpValidationProblemDetails) - mapeados para o que faz sentido mostrar,
// o resto cai num generico em vez de aparecer texto tecnico em ingles.
const IDENTITY_ERROR_MESSAGES: Record<string, string> = {
  DuplicateEmail: 'Já existe uma conta com este email.',
  DuplicateUserName: 'Já existe uma conta com este email.',
  InvalidEmail: 'Email inválido.',
  PasswordTooShort: 'A palavra-passe tem de ter pelo menos 6 caracteres.',
  PasswordRequiresNonAlphanumeric: 'A palavra-passe tem de incluir um caráter especial.',
  PasswordRequiresLower: 'A palavra-passe tem de incluir uma letra minúscula.',
  PasswordRequiresUpper: 'A palavra-passe tem de incluir uma letra maiúscula.',
  PasswordRequiresDigit: 'A palavra-passe tem de incluir um número.',
  InvalidToken: 'Código inválido ou expirado. Peça um código novo.',
}

function describeIdentityError(
  problem: components['schemas']['HttpValidationProblemDetails'] | undefined,
  fallback: string,
): string {
  const codes = Object.keys(problem?.errors ?? {})
  const known = codes.map(code => IDENTITY_ERROR_MESSAGES[code]).filter(Boolean)
  return known[0] ?? fallback
}

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
