import { describe, expect, it } from 'vitest'
import { describeIdentityError } from './identityErrors'

describe('describeIdentityError', () => {
  it('traduz um codigo de erro conhecido', () => {
    const message = describeIdentityError({ errors: { DuplicateEmail: ['ignored'] } }, 'fallback')
    expect(message).toBe('Já existe uma conta com este email.')
  })

  it('traduz erros de password para portugues', () => {
    expect(describeIdentityError({ errors: { PasswordTooShort: [] } }, 'fallback'))
      .toBe('A palavra-passe tem de ter pelo menos 6 caracteres.')
  })

  it('usa o primeiro codigo conhecido quando ha varios', () => {
    const message = describeIdentityError(
      { errors: { PasswordRequiresUpper: [], PasswordRequiresDigit: [] } },
      'fallback',
    )
    expect(message).toBe('A palavra-passe tem de incluir uma letra maiúscula.')
  })

  it('cai no fallback para codigo desconhecido', () => {
    expect(describeIdentityError({ errors: { AlgoNuncaVisto: [] } }, 'fallback')).toBe('fallback')
  })

  it('cai no fallback quando nao ha errors', () => {
    expect(describeIdentityError(undefined, 'fallback')).toBe('fallback')
    expect(describeIdentityError({ errors: {} }, 'fallback')).toBe('fallback')
  })
})
