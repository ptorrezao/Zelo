import { computed, ref, watch } from 'vue'
import { useRuntimeConfig } from '#app'
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
  color: string
  odometer: string
  registered: string
  nextInspection: string
  insurer: string
  insurancePolicyNumber: string
  insurancePeriodStart: string
  insurancePeriodEnd: string
  insurancePremium: string
  iucDueDate: string
}

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
const DOCUMENT_CATEGORY_TO_API: Record<VehicleDocument['category'], ApiDocument['category']> = {
  Seguro: 'Seguro',
  'Manutenção': 'Manutencao',
  'Inspeção': 'Inspecao',
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
    color: dto.color || '—',
    registered: fromIso(dto.registered),
    nextInspection: fromIso(dto.nextInspection),
    insurer: dto.insurer || '—',
    insurancePolicyNumber: dto.insurancePolicyNumber || '—',
    insurancePeriodStart: fromIso(dto.insurancePeriodStart),
    insurancePeriodEnd: fromIso(dto.insurancePeriodEnd),
    insurancePremium: dto.insurancePremium != null ? formatCostValue(Number(dto.insurancePremium)) : '—',
    iucDueDate: fromIso(dto.iucDueDate),
    odometer: formatKmValue(Number(dto.odometer)),
    photoUrl: dto.photoUrl ?? null,
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

// Marca por vehicleId (nao pelo tamanho dos arrays de detalhe - esse
// proxy parte-se assim que algo insere um item antes do fetch real
// acontecer, ex. addMaintenance). detailPromises tambem deduplica pedidos
// simultaneos quando varios componentes chamam loadVehicleDetail para o
// mesmo veiculo em paralelo.
const detailLoaded = new Set<string>()
const detailPromises = new Map<string, Promise<void>>()

// Os households do utilizador atual - ja nao ha um ID fixo para toda a
// app (GET /api/v1/households/me garante que existe sempre pelo menos
// um, criando-o na primeira chamada se for preciso). Por omissao usa-se
// o primeiro; selectedHouseholdId muda quando o utilizador troca no
// seletor (ver setHousehold), o que forca um refresh.
type HouseholdOption = components['schemas']['HouseholdResponse']
const myHouseholds = ref<HouseholdOption[]>([])
const selectedHouseholdId = ref('')
let householdsPromise: Promise<void> | null = null

async function resolveHouseholdId(client: ReturnType<typeof useApiClient>): Promise<string> {
  if (!householdsPromise) {
    householdsPromise = client.GET('/api/v1/households/me').then(({ data }) => {
      myHouseholds.value = data ?? []
      if (!selectedHouseholdId.value) {
        selectedHouseholdId.value = myHouseholds.value[0]?.id ?? ''
      }
    })
  }
  await householdsPromise
  if (!selectedHouseholdId.value) throw new Error('Não foi possível determinar o household do utilizador.')
  return selectedHouseholdId.value
}

async function loadVehicles(client: ReturnType<typeof useApiClient>) {
  const householdId = await resolveHouseholdId(client)
  const { data } = await client.GET('/api/auto/vehicles', { params: { query: { householdId } } })
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
  if (detailLoaded.has(vehicle.id)) return
  const inFlight = detailPromises.get(vehicle.id)
  if (inFlight) return inFlight

  const promise = (async () => {
    const [maintenances, documents] = await Promise.all([
      client.GET('/api/auto/vehicles/{vehicleId}/maintenances', { params: { path: { vehicleId: vehicle.id } } }),
      client.GET('/api/auto/vehicles/{vehicleId}/documents', { params: { path: { vehicleId: vehicle.id } } }),
    ])

    vehicle.maintenances = (maintenances.data ?? []).map(mapMaintenanceFromApi)
    vehicle.documents = (documents.data ?? []).map(mapDocumentFromApi)
    detailLoaded.add(vehicle.id)
  })()

  detailPromises.set(vehicle.id, promise)
  try {
    await promise
  } finally {
    detailPromises.delete(vehicle.id)
  }
}

export function useVehicles() {
  const client = useApiClient()

  if (!loadPromise) {
    loadPromise = loadVehicles(client)
  }

  // Forca um novo pedido a API, ignorando o cache do loadPromise - usado
  // depois de uma importacao de veiculos, que nao passa por addVehicle().
  function refresh() {
    // loadVehicles substitui os objetos Vehicle por novos (mesmos ids,
    // arrays de detalhe vazios) - sem isto, loadVehicleDetail via
    // detailLoaded pensava que o detalhe do veiculo ja estava carregado e
    // nunca voltava a pedir nada, deixando maintenances/documents vazios.
    detailLoaded.clear()
    detailPromises.clear()
    loadPromise = loadVehicles(client)
    return loadPromise
  }

  // Troca o household ativo (quando o utilizador tem mais que um) e
  // recarrega a lista de veiculos para esse household.
  function setHousehold(householdId: string) {
    if (householdId === selectedHouseholdId.value) return
    selectedHouseholdId.value = householdId
    selectedId.value = '' // o veiculo selecionado era doutro household
    void refresh()
  }

  const allVehicles = computed(() => groups.value.flatMap(group => group.items))
  const selected = computed(() => allVehicles.value.find(v => v.id === selectedId.value) ?? allVehicles.value[0])
  // Ate o pedido a API resolver (SSR e o primeiro render no browser),
  // selected.value nao existe - as apps mock nunca tinham este estado
  // porque os dados vinham sincronos.
  const photo = computed(() => selected.value ? photoFor(selected.value) : '')

  // watch, nao um "if" direto - selected e computed, por isso um if aqui
  // so corre uma vez, quando o composable e criado, e nunca mais quando o
  // utilizador troca de veiculo depois (selectedId muda, mas nada volta a
  // avaliar este bloco).
  watch(selected, (vehicle) => {
    if (vehicle) void loadVehicleDetail(client, vehicle)
  }, { immediate: true })

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

  // A foto (criacao, ou regeneracao ao mudar a cor - ver plano de geracao
  // automatica) demora dezenas de segundos a gerar no Worker, em
  // background - o pedido de create/update ja volta sem ela. Em vez de
  // obrigar a um refresh manual, pergunta-se de vez em quando ate
  // aparecer (ou desistir). Muta o vehicle.photoUrl diretamente -
  // groups e reativo, o <img> atualiza sozinho quando chegar.
  async function pollForPhoto(vehicleId: string, client: ReturnType<typeof useApiClient>) {
    for (let attempt = 0; attempt < 15; attempt++) {
      await new Promise(resolve => setTimeout(resolve, 4000))

      const vehicle = allVehicles.value.find(v => v.id === vehicleId)
      if (!vehicle || vehicle.photoUrl) return // saiu da lista, ou ja chegou por outra via

      const { data } = await client.GET('/api/auto/vehicles/{id}', { params: { path: { id: vehicleId } } })
      if (data?.photoUrl) {
        vehicle.photoUrl = data.photoUrl
        return
      }
    }
  }

  async function addVehicle(input: VehicleFormInput): Promise<Vehicle> {
    const householdId = await resolveHouseholdId(client)
    const { data, error } = await client.POST('/api/auto/vehicles', {
      params: { query: { householdId } },
      body: {
        category: input.category,
        brand: input.brand,
        model: input.model,
        plate: input.plate,
        vin: input.vin,
        color: input.color || null,
        driver: null,
        odometer: parseKmValue(input.odometer),
        registered: toIso(input.registered) ?? new Date().toISOString().slice(0, 10),
        nextInspection: toIso(input.nextInspection),
        insurer: input.insurer || null,
        insurancePolicyNumber: input.insurancePolicyNumber || null,
        insurancePeriodStart: toIso(input.insurancePeriodStart),
        insurancePeriodEnd: toIso(input.insurancePeriodEnd),
        insurancePremium: input.insurancePremium ? parseCostValue(input.insurancePremium) : null,
        iucDueDate: toIso(input.iucDueDate),
      },
    })
    if (!data) throw new Error(extractApiErrorMessage(error, 'Não foi possível criar o veículo.'))

    const vehicle = mapVehicleFromApi(data)
    const group = groups.value.find(g => g.label === input.category)
    group?.items.push(vehicle)
    if (!vehicle.photoUrl) void pollForPhoto(vehicle.id, client)
    return vehicle
  }

  async function updateVehicle(vehicleId: string, input: VehicleFormInput): Promise<Vehicle> {
    const { data, error } = await client.PUT('/api/auto/vehicles/{id}', {
      params: { path: { id: vehicleId } },
      body: {
        category: input.category,
        brand: input.brand,
        model: input.model,
        plate: input.plate,
        vin: input.vin,
        color: input.color || null,
        driver: null,
        odometer: parseKmValue(input.odometer),
        registered: toIso(input.registered) ?? new Date().toISOString().slice(0, 10),
        nextInspection: toIso(input.nextInspection),
        insurer: input.insurer || null,
        insurancePolicyNumber: input.insurancePolicyNumber || null,
        insurancePeriodStart: toIso(input.insurancePeriodStart),
        insurancePeriodEnd: toIso(input.insurancePeriodEnd),
        insurancePremium: input.insurancePremium ? parseCostValue(input.insurancePremium) : null,
        iucDueDate: toIso(input.iucDueDate),
      },
    })
    if (!data) throw new Error(extractApiErrorMessage(error, 'Não foi possível guardar as alterações.'))

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
    if (!updated.photoUrl) void pollForPhoto(updated.id, client)
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

  // Fluxo em 3 pedidos: 1) pede um URL pre-assinado de upload (o backend
  // gera a objectKey), 2) o browser envia o ficheiro diretamente para o
  // Garage nesse URL (nunca passa pela nossa Api), 3) confirma o registo
  // do documento com a objectKey devolvida em 1). O tipo (Pdf/Imagem) sai
  // do content-type do proprio ficheiro - o input no formulario ja
  // restringe a pdf/imagem via "accept", por isso qualquer outra coisa
  // cai em Imagem por omissao (nao deveria acontecer na pratica).
  async function addDocument(
    vehicleId: string, file: File, category: VehicleDocument['category'], date: string,
  ): Promise<VehicleDocument> {
    const contentType = file.type || 'application/octet-stream'
    const { data: uploadData, error: uploadError } = await client.POST('/api/auto/vehicles/{vehicleId}/documents/upload-url', {
      params: { path: { vehicleId } },
      body: { fileName: file.name, contentType },
    })
    const upload = uploadData as { objectKey: string, uploadUrl: string } | undefined
    if (!upload) throw new Error(extractApiErrorMessage(uploadError, 'Não foi possível preparar o envio do ficheiro.'))

    const putResponse = await fetch(upload.uploadUrl, { method: 'PUT', body: file, headers: { 'Content-Type': contentType } })
    if (!putResponse.ok) throw new Error('Não foi possível enviar o ficheiro.')

    const { data, error } = await client.POST('/api/auto/vehicles/{vehicleId}/documents', {
      params: { path: { vehicleId } },
      body: {
        objectKey: upload.objectKey,
        name: file.name,
        category: DOCUMENT_CATEGORY_TO_API[category],
        type: contentType === 'application/pdf' ? 'Pdf' : 'Imagem',
        date: toIso(date) ?? new Date().toISOString().slice(0, 10),
        sizeBytes: file.size,
      },
    })
    const created = data as ApiDocument | undefined
    if (!created) throw new Error(extractApiErrorMessage(error, 'Não foi possível registar o documento.'))

    const document = mapDocumentFromApi(created)
    const vehicle = allVehicles.value.find(v => v.id === vehicleId)
    vehicle?.documents.push(document)
    return document
  }

  return {
    groups,
    query,
    selectedId,
    isLoaded,
    refresh,
    myHouseholds,
    selectedHouseholdId,
    setHousehold,
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

// O backend devolve erros de validacao/negocio como { error: "mensagem" }
// (ver AutoEndpointHandlers, ex. "Matrícula já existe"). Sem schema
// tipado para respostas de erro, "error" vem como unknown - le-se a
// mensagem manualmente, com um fallback generico se a forma nao bater
// certo (ex. erro de rede, sem corpo JSON nenhum).
function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (error && typeof error === 'object' && 'error' in error && typeof (error as { error: unknown }).error === 'string') {
    return (error as { error: string }).error
  }
  return fallback
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

// Path absoluto normal ("/vehicles/...") so funciona quando a app vive na
// raiz do dominio - com routing por Path (NUXT_APP_BASE_URL=/auto/), o
// browser pedia-o em "/vehicles/..." (raiz do dominio, apanhado pela
// shell) em vez de "/auto/vehicles/...". Prefixar com o baseURL da app
// corrige isto nos dois casos (raiz e sub-path).
// A foto e gerada por veiculo real (nao por entrada de catalogo) e
// servida pela Api via URL pre-assinada com validade curta - ver
// VehicleResponse.PhotoUrl e plans/vehicle-image-generation.md. Sem
// PhotoUrl (geracao ainda a decorrer, ou falhou), VehiclePhoto mostra o
// placeholder de sempre.
function photoFor(vehicle: Vehicle) {
  return vehicle.photoUrl ?? ''
}

function logoFor(vehicle: Vehicle) {
  const baseURL = useRuntimeConfig().app.baseURL
  return `${baseURL}brands/${slugify(vehicle.brand)}.png`
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
