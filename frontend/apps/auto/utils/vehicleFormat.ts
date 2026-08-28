// Conversoes puras entre o formato de exibicao (dd/mm/aaaa, "24 780 km",
// "85,00") e o formato da API (ISO, numeros). Sem dependencias do Nuxt -
// de proposito, para serem testaveis com vitest simples.

// dd/mm/aaaa (formato do DatePicker, ver packages/ui/components/ui/DatePicker.vue) <-> aaaa-mm-dd (ISO, formato da API)
export function toIso(ddmmyyyy: string): string | null {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(ddmmyyyy.trim())
  if (!match) return null
  const [, day, month, year] = match
  return `${year}-${month}-${day}`
}

export function fromIso(iso: string | null | undefined): string {
  if (!iso) return '—'
  const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso)
  if (!match) return '—'
  const [, year, month, day] = match
  return `${day}/${month}/${year}`
}

export function formatCostValue(value: number): string {
  return value.toLocaleString('pt-PT', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function parseCostValue(value: string): number {
  return Number(value.replace(/\s/g, '').replace(/\./g, '').replace(',', '.')) || 0
}

export function formatKmValue(value: number): string {
  return `${value.toLocaleString('pt-PT')} km`
}

export function parseKmValue(value: string): number {
  return Number(value.replace(/[^\d]/g, '')) || 0
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  const units = ['KB', 'MB', 'GB']
  let value = bytes / 1024
  let unitIndex = 0
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }
  return `${value.toFixed(1)} ${units[unitIndex]}`
}
