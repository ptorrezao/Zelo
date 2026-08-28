import { describe, expect, it } from 'vitest'
import { formatBytes, formatCostValue, formatKmValue, fromIso, parseCostValue, parseKmValue, toIso } from './vehicleFormat'

describe('toIso', () => {
  it('converte dd/mm/aaaa para aaaa-mm-dd', () => {
    expect(toIso('15/06/2027')).toBe('2027-06-15')
  })

  it('devolve null para formato invalido', () => {
    expect(toIso('2027-06-15')).toBeNull()
    expect(toIso('')).toBeNull()
    expect(toIso('abc')).toBeNull()
  })

  it('ignora espacos a volta', () => {
    expect(toIso('  15/06/2027  ')).toBe('2027-06-15')
  })
})

describe('fromIso', () => {
  it('converte aaaa-mm-dd para dd/mm/aaaa', () => {
    expect(fromIso('2027-06-15')).toBe('15/06/2027')
  })

  it('aceita datas com hora (so usa a parte da data)', () => {
    expect(fromIso('2027-06-15T00:00:00Z')).toBe('15/06/2027')
  })

  it('devolve travessao para valores vazios', () => {
    expect(fromIso(null)).toBe('—')
    expect(fromIso(undefined)).toBe('—')
    expect(fromIso('')).toBe('—')
  })

  it('devolve travessao para formato invalido', () => {
    expect(fromIso('nao e uma data')).toBe('—')
  })
})

describe('toIso / fromIso', () => {
  it('sao inversas uma da outra', () => {
    expect(fromIso(toIso('01/12/2026'))).toBe('01/12/2026')
  })
})

describe('formatCostValue / parseCostValue', () => {
  it('formata um numero como custo em pt-PT', () => {
    expect(formatCostValue(85)).toBe('85,00')
    expect(formatCostValue(1234.5)).toBe('1234,50')
  })

  it('faz o parse de volta para numero', () => {
    expect(parseCostValue('85,00')).toBe(85)
    expect(parseCostValue('1234,50')).toBe(1234.5)
  })

  it('parseCostValue devolve 0 para texto invalido', () => {
    expect(parseCostValue('abc')).toBe(0)
    expect(parseCostValue('')).toBe(0)
  })
})

describe('formatKmValue / parseKmValue', () => {
  it('formata quilometros com unidade', () => {
    // toLocaleString('pt-PT') usa espaco insecavel como separador de
    // milhares (nao um espaco normal) - regex evita depender do caracter exato.
    expect(formatKmValue(24780)).toMatch(/^24.780 km$/)
  })

  it('faz o parse de volta para numero, ignorando texto', () => {
    expect(parseKmValue(formatKmValue(24780))).toBe(24780)
    expect(parseKmValue('0 km')).toBe(0)
  })

  it('parseKmValue devolve 0 para texto sem digitos', () => {
    expect(parseKmValue('km')).toBe(0)
  })
})

describe('formatBytes', () => {
  it('mostra bytes sem conversao abaixo de 1024', () => {
    expect(formatBytes(512)).toBe('512 B')
  })

  it('converte para KB/MB/GB conforme a grandeza', () => {
    expect(formatBytes(2048)).toBe('2.0 KB')
    expect(formatBytes(1_500_000)).toBe('1.4 MB')
    expect(formatBytes(1_500_000_000)).toBe('1.4 GB')
  })

  it('nao ultrapassa GB mesmo com valores enormes', () => {
    expect(formatBytes(1_500_000_000_000)).toMatch(/GB$/)
  })
})
