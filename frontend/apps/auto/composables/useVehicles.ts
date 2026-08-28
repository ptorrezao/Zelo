import { computed, ref } from 'vue'
import { useApiClient } from '@zelo/ui/composables/useApiClient'
import type { components } from '@zelo/api-client'
import type { Maintenance, Vehicle, VehicleDocument, VehicleGroup } from '../types/vehicle'
import { formatBytes, formatCostValue, formatKmValue, fromIso, parseCostValue, parseKmValue, toIso } from '../utils/vehicleFormat'

export interface VehicleFormInput {
  category: 'Motociclos' | 'Ligeiros'
  brand: string
  model: string
  plate: string
  vin: string
  odometer: string
  registered: string
  nextInspection: string
  insurer: string
  insuranceRenewal: string
  iucDueDate: string
}

// TODO: nao ha ainda forma de um utilizador saber o seu proprio household
// (nenhum endpoint em Identity expoe isto, e o registo nao cria um
// Household). Fixo por agora - trocar assim que essa peca existir.
const DEFAULT_HOUSEHOLD_ID = '11111111-1111-1111-1111-111111111111'

type ApiVehicle = components['schemas']['VehicleResponse']
type ApiMaintenance = components['schemas']['MaintenanceResponse']
type ApiDocument = components['schemas']['DocumentResponse']

const DOCUMENT_CATEGORY_FROM_API: Record<ApiDocument['category'], VehicleDocument['category']> = {
  Seguro: 'Seguro',
  Manutencao: 'Manutenção',
  Inspecao: 'Inspeção',
  Registo: 'Registo',
  Fatura: 'Fatura',
}
function mapVehicleFromApi(dto: ApiVehicle): Vehicle {
  return {
    id: dto.id,
    driver: dto.driver ?? '—',
    brand: dto.brand,
    model: dto.model,
    plate: dto.plate,
    status: dto.status,
    vin: dto.vin || '—',
    registered: fromIso(dto.registered),
    nextInspection: fromIso(dto.nextInspection),
    insurer: dto.insurer || '—',
    insuranceRenewal: fromIso(dto.insuranceRenewal),
    iucDueDate: fromIso(dto.iucDueDate),
    odometer: formatKmValue(Number(dto.odometer)),
    maintenances: [],
    documents: [],
    // O endpoint /stats nao devolve consumo medio nem o detalhe mensal -
    // fica a 0/vazio ate essa parte da API existir, para nao inventar
    // numeros que ninguem calculou.
    stats: { kmsLastMonth: 0, avgConsumption: 0, avgKmPerDay: 0, maintenanceCostLastMonth: 0, monthlyKms: [] },
  }
}

function mapMaintenanceFromApi(dto: ApiMaintenance): Maintenance {
  return {
    id: dto.id,
    date: fromIso(dto.date),
    odometer: formatKmValue(Number(dto.odometer)),
    workshop: dto.workshop,
    description: dto.description,
    cost: formatCostValue(Number(dto.cost)),
    type: dto.type.toLowerCase() as Maintenance['type'],
    items: dto.items.length > 0
      ? dto.items.map(i => ({ description: i.description, price: formatCostValue(Number(i.price)), serialNumber: i.serialNumber ?? undefined }))
      : undefined,
    invoice: dto.invoiceNumber
      ? { number: dto.invoiceNumber, date: fromIso(dto.invoiceDate), url: '' }
      : undefined,
  }
}

function mapDocumentFromApi(dto: ApiDocument): VehicleDocument {
  return {
    id: dto.id,
    name: dto.name,
    category: DOCUMENT_CATEGORY_FROM_API[dto.category],
    type: dto.type.toLowerCase() as VehicleDocument['type'],
    date: fromIso(dto.date),
    size: formatBytes(Number(dto.sizeBytes)),
  }
}

const groups = ref<VehicleGroup[]>([{ label: 'Motociclos', items: [] }, { label: 'Ligeiros', items: [] }])
const query = ref('')
const selectedId = ref('')
const isLoaded = ref(false)
let loadPromise: Promise<void> | null = null

async function loadVehicles(client: ReturnType<typeof useApiClient>) {
  const { data } = await client.GET('/api/auto/vehicles', { params: { query: { householdId: DEFAULT_HOUSEHOLD_ID } } })
  const vehicles = (data ?? []).map(mapVehicleFromApi)

  groups.value = [
    { label: 'Motociclos', items: vehicles.filter((_, i) => (data ?? [])[i].category === 'Motociclos') },
    { label: 'Ligeiros', items: vehicles.filter((_, i) => (data ?? [])[i].category === 'Ligeiros') },
  ]

  if (!selectedId.value && vehicles.length > 0) {
    selectedId.value = vehicles[0].id
  }

  // Detalhe (manutencoes/documentos) so vem depois, um pedido por veiculo
  // selecionado - listar tudo a partida seria caro sem necessidade.
  isLoaded.value = true
}

async function loadVehicleDetail(client: ReturnType<typeof useApiClient>, vehicle: Vehicle) {
  if (vehicle.maintenances.length > 0 || vehicle.documents.length > 0) return // ja carregado

  const [maintenances, documents] = await Promise.all([
    client.GET('/api/auto/vehicles/{vehicleId}/maintenances', { params: { path: { vehicleId: vehicle.id } } }),
    client.GET('/api/auto/vehicles/{vehicleId}/documents', { params: { path: { vehicleId: vehicle.id } } }),
  ])

  vehicle.maintenances = (maintenances.data ?? []).map(mapMaintenanceFromApi)
  vehicle.documents = (documents.data ?? []).map(mapDocumentFromApi)
}

export function useVehicles() {
  const client = useApiClient()

  if (!loadPromise) {
    loadPromise = loadVehicles(client)
  }

  const allVehicles = computed(() => groups.value.flatMap(group => group.items))
  const selected = computed(() => allVehicles.value.find(v => v.id === selectedId.value) ?? allVehicles.value[0])
  // Ate o pedido a API resolver (SSR e o primeiro render no browser),
  // selected.value nao existe - as apps mock nunca tinham este estado
  // porque os dados vinham sincronos.
  const photo = computed(() => selected.value ? photoFor(selected.value) : '')

  if (selected.value) {
    void loadVehicleDetail(client, selected.value)
  }

  const visibleGroups = computed(() => {
    const term = query.value.trim().toLowerCase()
    return groups.value
      .map(group => ({
        ...group,
        items: term
          ? group.items.filter(v =>
              fullName(v).toLowerCase().includes(term) || v.plate.toLowerCase().includes(term),
            )
          : group.items,
      }))
      .filter(group => group.items.length > 0)
  })

  async function findMaintenance(maintenanceId: string): Promise<{ vehicle: Vehicle, maintenance: Maintenance } | undefined> {
    for (const vehicle of allVehicles.value) {
      await loadVehicleDetail(client, vehicle)
      const maintenance = vehicle.maintenances.find(m => m.id === maintenanceId)
      if (maintenance) {
        return { vehicle, maintenance }
      }
    }
    return undefined
  }

  function categoryOf(vehicleId: string) {
    return groups.value.find(g => g.items.some(v => v.id === vehicleId))?.label as 'Motociclos' | 'Ligeiros' | undefined
  }

  async function addVehicle(input: VehicleFormInput): Promise<Vehicle> {
    const { data } = await client.POST('/api/auto/vehicles', {
      params: { query: { householdId: DEFAULT_HOUSEHOLD_ID } },
      body: {
        category: input.category,
        brand: input.brand,
        model: input.model,
        plate: input.plate,
        vin: input.vin,
        driver: null,
        odometer: parseKmValue(input.odometer),
        registered: toIso(input.registered) ?? new Date().toISOString().slice(0, 10),
        nextInspection: toIso(input.nextInspection),
        insurer: input.insurer || null,
        insuranceRenewal: toIso(input.insuranceRenewal),
        iucDueDate: toIso(input.iucDueDate),
      },
    })

    const vehicle = mapVehicleFromApi(data!)
    const group = groups.value.find(g => g.label === input.category)
    group?.items.push(vehicle)
    return vehicle
  }

  async function updateVehicle(vehicleId: string, input: VehicleFormInput): Promise<Vehicle | undefined> {
    const { data } = await client.PUT('/api/auto/vehicles/{id}', {
      params: { path: { id: vehicleId } },
      body: {
        category: input.category,
        brand: input.brand,
        model: input.model,
        plate: input.plate,
        vin: input.vin,
        driver: null,
        odometer: parseKmValue(input.odometer),
        registered: toIso(input.registered) ?? new Date().toISOString().slice(0, 10),
        nextInspection: toIso(input.nextInspection),
        insurer: input.insurer || null,
        insuranceRenewal: toIso(input.insuranceRenewal),
        iucDueDate: toIso(input.iucDueDate),
      },
    })
    if (!data) return undefined

    const updated = mapVehicleFromApi(data)
    const currentCategory = categoryOf(vehicleId)
    const fromGroup = groups.value.find(g => g.label === currentCategory)
    const existingIndex = fromGroup?.items.findIndex(v => v.id === vehicleId) ?? -1
    if (fromGroup && existingIndex !== -1) {
      const [existing] = fromGroup.items.splice(existingIndex, 1)
      updated.maintenances = existing.maintenances
      updated.documents = existing.documents
    }

    const toGroup = groups.value.find(g => g.label === input.category)
    toGroup?.items.push(updated)
    return updated
  }

  async function addMaintenance(
    vehicleId: string,
    input: { date: string, type: Maintenance['type'], workshop: string, description: string, cost: string, odometer: string },
  ): Promise<Maintenance | undefined> {
    const { data } = await client.POST('/api/auto/vehicles/{vehicleId}/maintenances', {
      params: { path: { vehicleId } },
      body: {
        date: toIso(input.date) ?? new Date().toISOString().slice(0, 10),
        odometer: parseKmValue(input.odometer),
        workshop: input.workshop,
        description: input.description,
        type: (input.type.charAt(0).toUpperCase() + input.type.slice(1)) as ApiMaintenance['type'],
        cost: parseCostValue(input.cost),
        invoiceNumber: null,
        invoiceDate: null,
        items: null,
      },
    })
    if (!data) return undefined

    const maintenance = mapMaintenanceFromApi(data)
    const vehicle = allVehicles.value.find(v => v.id === vehicleId)
    vehicle?.maintenances.unshift(maintenance)
    return maintenance
  }

  function addDocument(vehicleId: string, document: VehicleDocument) {
    // TODO: sem UI de upload ainda - quando existir, chamar
    // /documents/upload-url, fazer o PUT, e so depois confirmar aqui via
    // POST /documents com o objectKey devolvido.
    const vehicle = allVehicles.value.find(v => v.id === vehicleId)
    vehicle?.documents.push(document)
  }

  return {
    groups,
    query,
    selectedId,
    isLoaded,
    allVehicles,
    selected,
    photo,
    visibleGroups,
    fullName,
    photoFor,
    logoFor,
    formatConsumption,
    formatKms,
    formatCost,
    findMaintenance,
    categoryOf,
    addVehicle,
    updateVehicle,
    addMaintenance,
    addDocument,
  }
}

function slugify(value: string) {
  return value
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '')
}

function fullName(vehicle: Vehicle) {
  return `${vehicle.brand} ${vehicle.model}`
}

function photoFor(vehicle: Vehicle) {
  return `/vehicles/${slugify(fullName(vehicle))}.png`
}

function logoFor(vehicle: Vehicle) {
  return `/brands/${slugify(vehicle.brand)}.png`
}

function formatConsumption(value: number) {
  return `${value.toFixed(1)} l/100km`
}

function formatKms(value: number) {
  return value.toLocaleString('pt-PT') + ' km'
}

function formatCost(value: number) {
  return value.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })
}
