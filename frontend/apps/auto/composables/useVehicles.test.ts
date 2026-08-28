import { beforeEach, describe, expect, it, vi } from 'vitest'

const client = { GET: vi.fn(), POST: vi.fn(), PUT: vi.fn() }
vi.mock('@zelo/ui/composables/useApiClient', () => ({
  useApiClient: () => client,
}))

function apiVehicle(overrides: Partial<Record<string, unknown>> = {}) {
  return {
    id: 'v1',
    category: 'Ligeiros',
    brand: 'Toyota',
    model: 'Corolla',
    plate: 'AA-00-BB',
    vin: 'VIN123',
    status: 'Ativo',
    driver: null,
    odometer: 12000,
    registered: '2020-01-01',
    nextInspection: null,
    insurer: null,
    insuranceRenewal: null,
    iucDueDate: null,
    ...overrides,
  }
}

describe('useVehicles', () => {
  beforeEach(() => {
    vi.resetModules()
    client.GET.mockReset()
    client.POST.mockReset()
    client.PUT.mockReset()
    client.GET.mockResolvedValue({ data: [] })
  })

  it('carrega veiculos e agrupa por categoria', async () => {
    client.GET.mockImplementation((path: string) => {
      if (path === '/api/auto/vehicles')
        return Promise.resolve({ data: [apiVehicle({ id: 'v1', category: 'Ligeiros' }), apiVehicle({ id: 'v2', category: 'Motociclos', brand: 'Honda' })] })
      return Promise.resolve({ data: [] })
    })

    const { useVehicles } = await import('./useVehicles')
    const { allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    expect(allVehicles.value).toHaveLength(2)
  })

  it('seleciona automaticamente o primeiro veiculo carregado', async () => {
    client.GET.mockImplementation((path: string) => {
      if (path === '/api/auto/vehicles')
        return Promise.resolve({ data: [apiVehicle({ id: 'v1' })] })
      return Promise.resolve({ data: [] })
    })

    const { useVehicles } = await import('./useVehicles')
    const { selectedId, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    expect(selectedId.value).toBe('v1')
  })

  it('visibleGroups filtra por nome ou matricula', async () => {
    client.GET.mockImplementation((path: string) => {
      if (path === '/api/auto/vehicles')
        return Promise.resolve({ data: [apiVehicle({ id: 'v1', brand: 'Toyota', plate: 'AA-00-BB' }), apiVehicle({ id: 'v2', brand: 'Honda', plate: 'CC-11-DD' })] })
      return Promise.resolve({ data: [] })
    })

    const { useVehicles } = await import('./useVehicles')
    const { query, visibleGroups, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    query.value = 'honda'
    const items = visibleGroups.value.flatMap(g => g.items)
    expect(items).toHaveLength(1)
    expect(items[0].brand).toBe('Honda')
  })

  it('addVehicle publica no grupo correto', async () => {
    client.POST.mockResolvedValue({ data: apiVehicle({ id: 'novo', category: 'Motociclos', brand: 'BMW' }) })

    const { useVehicles } = await import('./useVehicles')
    const { addVehicle, groups, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    const created = await addVehicle({
      category: 'Motociclos', brand: 'BMW', model: 'F800', plate: 'EE-22-FF', vin: 'VIN',
      odometer: '0 km', registered: '01/01/2026', nextInspection: '', insurer: '', insuranceRenewal: '', iucDueDate: '',
    })

    expect(created.brand).toBe('BMW')
    const motos = groups.value.find(g => g.label === 'Motociclos')
    expect(motos?.items.some(v => v.id === 'novo')).toBe(true)
  })

  it('addMaintenance adiciona a manutencao ao veiculo', async () => {
    client.GET.mockImplementation((path: string) => {
      if (path === '/api/auto/vehicles')
        return Promise.resolve({ data: [apiVehicle({ id: 'v1' })] })
      return Promise.resolve({ data: [] })
    })
    client.POST.mockResolvedValue({
      data: {
        id: 'm1', vehicleId: 'v1', date: '2026-01-01', odometer: 12500, workshop: 'Oficina X',
        description: 'Revisao', type: 'Preventiva', cost: 80, invoiceNumber: null, invoiceDate: null, items: [],
      },
    })

    const { useVehicles } = await import('./useVehicles')
    const { addMaintenance, allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    const maintenance = await addMaintenance('v1', {
      date: '01/01/2026', type: 'preventiva', workshop: 'Oficina X', description: 'Revisao', cost: '80,00', odometer: '12 500 km',
    })

    expect(maintenance?.workshop).toBe('Oficina X')
    expect(allVehicles.value.find(v => v.id === 'v1')?.maintenances).toHaveLength(1)
  })

  it('addDocument adiciona o documento ao veiculo', async () => {
    client.GET.mockImplementation((path: string) => {
      if (path === '/api/auto/vehicles')
        return Promise.resolve({ data: [apiVehicle({ id: 'v1' })] })
      return Promise.resolve({ data: [] })
    })

    const { useVehicles } = await import('./useVehicles')
    const { addDocument, allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    addDocument('v1', { id: 'd1', name: 'Apolice.pdf', category: 'Seguro', type: 'pdf', date: '01/01/2026', size: '1.0 KB' })

    expect(allVehicles.value.find(v => v.id === 'v1')?.documents).toHaveLength(1)
  })
})
