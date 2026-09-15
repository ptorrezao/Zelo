import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('#app', () => ({
  useRuntimeConfig: () => ({ app: { baseURL: '/' } }),
}))

const client = { GET: vi.fn(), POST: vi.fn(), PUT: vi.fn() }
vi.mock('@zelo/ui/composables/useApiClient', () => ({
  useApiClient: () => client,
}))

const DEFAULT_HOUSEHOLD = { id: 'h1', name: 'Casa', role: 'Owner' }

// resolveHouseholdId chama sempre /api/v1/households/me primeiro - os
// testes so precisam de configurar o que interessa (/api/auto/vehicles,
// etc.), esta funcao trata do resto por omissao.
function mockGet(paths: Record<string, unknown>) {
  client.GET.mockImplementation((path: string) => {
    if (path === '/api/v1/households/me') return Promise.resolve({ data: [DEFAULT_HOUSEHOLD] })
    if (path in paths) return Promise.resolve({ data: paths[path] })
    return Promise.resolve({ data: [] })
  })
}

function apiVehicle(overrides: Partial<Record<string, unknown>> = {}) {
  return {
    id: 'v1',
    category: 'Ligeiros',
    brand: 'Toyota',
    model: 'Corolla',
    plate: 'AA-00-BB',
    vin: 'VIN123',
    color: null,
    status: 'Ativo',
    driver: null,
    odometer: 12000,
    registered: '2020-01-01',
    nextInspection: null,
    insurer: null,
    insurancePolicyNumber: null,
    insurancePeriodStart: null,
    insurancePeriodEnd: null,
    insurancePremium: null,
    iucDueDate: null,
    photoUrl: null,
    ...overrides,
  }
}

describe('useVehicles', () => {
  beforeEach(() => {
    vi.resetModules()
    client.GET.mockReset()
    client.POST.mockReset()
    client.PUT.mockReset()
    mockGet({})
  })

  it('carrega veiculos e agrupa por categoria', async () => {
    mockGet({
      '/api/auto/vehicles': [apiVehicle({ id: 'v1', category: 'Ligeiros' }), apiVehicle({ id: 'v2', category: 'Motociclos', brand: 'Honda' })],
    })

    const { useVehicles } = await import('./useVehicles')
    const { allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    expect(allVehicles.value).toHaveLength(2)
  })

  it('mapeia photoUrl da API, ou null quando a foto ainda nao existe', async () => {
    mockGet({
      '/api/auto/vehicles': [apiVehicle({ id: 'v1', photoUrl: 'https://storage.local/vehicles/v1/photo.png' }), apiVehicle({ id: 'v2', photoUrl: null })],
    })

    const { useVehicles } = await import('./useVehicles')
    const { allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    expect(allVehicles.value.find(v => v.id === 'v1')?.photoUrl).toBe('https://storage.local/vehicles/v1/photo.png')
    expect(allVehicles.value.find(v => v.id === 'v2')?.photoUrl).toBeNull()
  })

  it('seleciona automaticamente o primeiro veiculo carregado', async () => {
    mockGet({ '/api/auto/vehicles': [apiVehicle({ id: 'v1' })] })

    const { useVehicles } = await import('./useVehicles')
    const { selectedId, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    expect(selectedId.value).toBe('v1')
  })

  it('visibleGroups filtra por nome ou matricula', async () => {
    mockGet({
      '/api/auto/vehicles': [apiVehicle({ id: 'v1', brand: 'Toyota', plate: 'AA-00-BB' }), apiVehicle({ id: 'v2', brand: 'Honda', plate: 'CC-11-DD' })],
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
      category: 'Motociclos', brand: 'BMW', model: 'F800', plate: 'EE-22-FF', vin: 'VIN', color: 'Preto',
      odometer: '0 km', registered: '01/01/2026', nextInspection: '', insurer: '',
      insurancePolicyNumber: '', insurancePeriodStart: '', insurancePeriodEnd: '', insurancePremium: '', iucDueDate: '',
    })

    expect(created.brand).toBe('BMW')
    const motos = groups.value.find(g => g.label === 'Motociclos')
    expect(motos?.items.some(v => v.id === 'novo')).toBe(true)
  })

  it('addVehicle lanca a mensagem de erro do backend (ex. matricula duplicada) em vez de rebentar', async () => {
    client.POST.mockResolvedValue({ error: { error: 'Matrícula já existe neste household.' } })

    const { useVehicles } = await import('./useVehicles')
    const { addVehicle, groups, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    await expect(addVehicle({
      category: 'Ligeiros', brand: 'Toyota', model: 'Corolla', plate: 'AA-00-BB', vin: 'VIN', color: 'Branco',
      odometer: '0 km', registered: '01/01/2026', nextInspection: '', insurer: '',
      insurancePolicyNumber: '', insurancePeriodStart: '', insurancePeriodEnd: '', insurancePremium: '', iucDueDate: '',
    })).rejects.toThrow('Matrícula já existe neste household.')
    expect(groups.value.every(g => g.items.every(v => v.plate !== 'AA-00-BB'))).toBe(true)
  })

  it('updateVehicle lanca a mensagem de erro do backend em vez de fechar silenciosamente', async () => {
    mockGet({ '/api/auto/vehicles': [apiVehicle({ id: 'v1' })] })
    client.PUT.mockResolvedValue({ error: { error: 'Cor é obrigatória.' } })

    const { useVehicles } = await import('./useVehicles')
    const { updateVehicle, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    await expect(updateVehicle('v1', {
      category: 'Ligeiros', brand: 'Toyota', model: 'Corolla', plate: 'AA-00-BB', vin: 'VIN123', color: '',
      odometer: '12 000 km', registered: '01/01/2020', nextInspection: '', insurer: '',
      insurancePolicyNumber: '', insurancePeriodStart: '', insurancePeriodEnd: '', insurancePremium: '', iucDueDate: '',
    })).rejects.toThrow('Cor é obrigatória.')
  })

  it('addMaintenance adiciona a manutencao ao veiculo', async () => {
    mockGet({ '/api/auto/vehicles': [apiVehicle({ id: 'v1' })] })
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

  it('trocar de veiculo selecionado carrega o detalhe do novo veiculo', async () => {
    mockGet({
      '/api/auto/vehicles': [apiVehicle({ id: 'v1' }), apiVehicle({ id: 'v2', brand: 'Honda' })],
      '/api/auto/vehicles/{vehicleId}/maintenances': [],
      '/api/auto/vehicles/{vehicleId}/documents': [],
    })

    const { useVehicles } = await import('./useVehicles')
    const { selectedId, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))
    await vi.waitFor(() => expect(client.GET).toHaveBeenCalledWith(
      '/api/auto/vehicles/{vehicleId}/maintenances', { params: { path: { vehicleId: 'v1' } } },
    ))

    selectedId.value = 'v2'

    await vi.waitFor(() => expect(client.GET).toHaveBeenCalledWith(
      '/api/auto/vehicles/{vehicleId}/maintenances', { params: { path: { vehicleId: 'v2' } } },
    ))
  })

  it('addMaintenance antes do detalhe carregar nao bloqueia o fetch seguinte (nao usa length como proxy)', async () => {
    let resolveMaintenances: (value: { data: unknown[] }) => void
    const maintenancesPromise = new Promise<{ data: unknown[] }>((resolve) => { resolveMaintenances = resolve })

    client.GET.mockImplementation((path: string) => {
      if (path === '/api/v1/households/me') return Promise.resolve({ data: [DEFAULT_HOUSEHOLD] })
      if (path === '/api/auto/vehicles') return Promise.resolve({ data: [apiVehicle({ id: 'v1' })] })
      if (path === '/api/auto/vehicles/{vehicleId}/maintenances') return maintenancesPromise
      return Promise.resolve({ data: [] })
    })
    client.POST.mockResolvedValue({
      data: {
        id: 'novo', vehicleId: 'v1', date: '2026-02-01', odometer: 13000, workshop: 'Oficina Y',
        description: 'Adicionada durante o carregamento', type: 'Preventiva', cost: 40, invoiceNumber: null, invoiceDate: null, items: [],
      },
    })

    const { useVehicles } = await import('./useVehicles')
    const { addMaintenance, findMaintenance, allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    // O GET de manutencoes ainda esta pendente - adiciona uma manutencao
    // antes dele resolver, o que faz vehicle.maintenances.length passar
    // de 0 para 1 (o proxy antigo confundia isto com "ja carregado").
    await addMaintenance('v1', {
      date: '01/02/2026', type: 'preventiva', workshop: 'Oficina Y', description: 'Adicionada durante o carregamento', cost: '40,00', odometer: '13 000 km',
    })
    expect(allVehicles.value.find(v => v.id === 'v1')?.maintenances).toHaveLength(1)

    resolveMaintenances!({
      data: [{
        id: 'historico', vehicleId: 'v1', date: '2025-01-01', odometer: 5000, workshop: 'Oficina Antiga',
        description: 'Revisao antiga', type: 'Preventiva', cost: 60, invoiceNumber: null, invoiceDate: null, items: [],
      }],
    })

    // findMaintenance forca outro loadVehicleDetail - se a guarda antiga
    // (length > 0) ainda existisse, isto seria um no-op e o item
    // historico nunca apareceria.
    const found = await findMaintenance('historico')
    expect(found?.maintenance.workshop).toBe('Oficina Antiga')
  })

  it('pollForPhoto atualiza vehicle.photoUrl quando a foto fica pronta em background', async () => {
    vi.useFakeTimers()
    try {
      mockGet({ '/api/auto/vehicles': [apiVehicle({ id: 'v1', photoUrl: null })] })
      client.PUT.mockResolvedValue({ data: apiVehicle({ id: 'v1', color: 'Azul', photoUrl: null }) })
      client.GET.mockImplementation((path: string, opts?: { params?: { path?: { id?: string } } }) => {
        if (path === '/api/v1/households/me') return Promise.resolve({ data: [DEFAULT_HOUSEHOLD] })
        if (path === '/api/auto/vehicles') return Promise.resolve({ data: [apiVehicle({ id: 'v1', photoUrl: null })] })
        if (path === '/api/auto/vehicles/{id}' && opts?.params?.path?.id === 'v1') {
          return Promise.resolve({ data: apiVehicle({ id: 'v1', photoUrl: 'https://storage.local/v1/photo.png' }) })
        }
        return Promise.resolve({ data: [] })
      })

      const { useVehicles } = await import('./useVehicles')
      const { updateVehicle, allVehicles, isLoaded } = useVehicles()
      await vi.waitFor(() => expect(isLoaded.value).toBe(true), { timeout: 5000 })

      await updateVehicle('v1', {
        category: 'Ligeiros', brand: 'Toyota', model: 'Corolla', plate: 'AA-00-BB', vin: 'VIN123', color: 'Azul',
        odometer: '12 000 km', registered: '01/01/2020', nextInspection: '', insurer: '',
        insurancePolicyNumber: '', insurancePeriodStart: '', insurancePeriodEnd: '', insurancePremium: '', iucDueDate: '',
      })
      expect(allVehicles.value.find(v => v.id === 'v1')?.photoUrl).toBeNull()

      await vi.advanceTimersByTimeAsync(4000)

      expect(allVehicles.value.find(v => v.id === 'v1')?.photoUrl).toBe('https://storage.local/v1/photo.png')
    } finally {
      vi.useRealTimers()
    }
  })

  it('addDocument faz upload (upload-url -> PUT -> confirmar) e adiciona o documento ao veiculo', async () => {
    mockGet({ '/api/auto/vehicles': [apiVehicle({ id: 'v1' })] })
    client.POST.mockImplementation((path: string) => {
      if (path === '/api/auto/vehicles/{vehicleId}/documents/upload-url') {
        return Promise.resolve({ data: { objectKey: 'vehicles/v1/abc-Apolice.pdf', uploadUrl: 'https://storage.local/upload', expiresAt: '2026-01-01T00:00:00Z' } })
      }
      if (path === '/api/auto/vehicles/{vehicleId}/documents') {
        return Promise.resolve({
          data: {
            id: 'd1', name: 'Apolice.pdf', category: 'Seguro', type: 'Pdf', date: '2026-01-01', sizeBytes: 1024, downloadUrl: 'https://storage.local/read/d1',
          },
        })
      }
      return Promise.resolve({ data: [] })
    })
    const fetchMock = vi.fn().mockResolvedValue({ ok: true })
    vi.stubGlobal('fetch', fetchMock)

    const { useVehicles } = await import('./useVehicles')
    const { addDocument, allVehicles, isLoaded } = useVehicles()
    await vi.waitFor(() => expect(isLoaded.value).toBe(true))

    const file = new File(['conteudo'], 'Apolice.pdf', { type: 'application/pdf' })
    const document = await addDocument('v1', file, 'Seguro', '01/01/2026')

    expect(document.name).toBe('Apolice.pdf')
    expect(document.downloadUrl).toBe('https://storage.local/read/d1')
    expect(fetchMock).toHaveBeenCalledWith('https://storage.local/upload', expect.objectContaining({ method: 'PUT' }))
    expect(allVehicles.value.find(v => v.id === 'v1')?.documents).toHaveLength(1)
    vi.unstubAllGlobals()
  })
})
