import type { components } from '@zelo/api-client'

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

export function describeIdentityError(
  problem: components['schemas']['HttpValidationProblemDetails'] | undefined,
  fallback: string,
): string {
  const codes = Object.keys(problem?.errors ?? {})
  const known = codes.map(code => IDENTITY_ERROR_MESSAGES[code]).filter(Boolean)
  return known[0] ?? fallback
}
