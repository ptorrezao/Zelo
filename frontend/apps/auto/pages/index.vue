<script setup lang="ts">
interface Maintenance {
  date: string
  odometer: string
  workshop: string
  description: string
  cost: string
  type: 'preventiva' | 'corretiva'
}

interface VehicleStats {
  kmsLastMonth: number
  avgConsumption: number
  avgKmPerDay: number
  maintenanceCostLastMonth: number
  monthlyKms: { label: string, value: number, reference: number }[]
}

// Ao contrario das medidas do modelo, tudo isto e de cada viatura: o VIN, os
// quilometros e as datas nao se herdam do catalogo.
interface Vehicle {
  id: string
  driver: string
  // Separados para o logotipo sair da marca sem a adivinhar a partir da
  // primeira palavra — "Land Rover Defender" daria "Land".
  brand: string
  model: string
  plate: string
  status: string
  vin: string
  registered: string
  nextInspection: string
  insurer: string
  insuranceRenewal: string
  odometer: string
  maintenances: Maintenance[]
  stats: VehicleStats
  alert?: boolean
  online?: boolean
}

const groups: { label: string, items: Vehicle[] }[] = [
  {
    label: 'Favoritos',
    items: [
    ],
  },
  {
    label: 'Motociclos',
    items: [
      {
        id: '236-542-010',
        driver: 'Pedro Torrezão',
        brand: 'Yamaha',
        model: 'Tenere',
        plate: 'BA-07-NT',
        status: 'A caminho',
        online: true,
        vin: 'JYARM09E4LA004178',
        registered: '15/06/2021',
        nextInspection: '15/06/2027',
        insurer: 'Fidelidade',
        insuranceRenewal: '01/09/2026',
        odometer: '24 780 km',
        maintenances: [
          { date: '12/09/2026', odometer: '24 650 km', workshop: 'Auto Serviço Silva', description: 'Troca de óleo e filtros', cost: '85,00', type: 'preventiva' },
          { date: '28/08/2026', odometer: '24 420 km', workshop: 'Manutenção Total', description: 'Inspeção geral', cost: '45,00', type: 'preventiva' },
          { date: '15/06/2026', odometer: '23 980 km', workshop: 'Auto Serviço Silva', description: 'Reparação da correia', cost: '280,00', type: 'corretiva' },
        ],
        stats: {
          kmsLastMonth: 850,
          avgConsumption: 4.2,
          avgKmPerDay: 28.3,
          maintenanceCostLastMonth: 130,
          monthlyKms: [
            { label: 'Set 25', value: 720, reference: 775 },
            { label: 'Out 25', value: 820, reference: 775 },
            { label: 'Nov 25', value: 680, reference: 775 },
            { label: 'Dez 25', value: 620, reference: 775 },
            { label: 'Jan 26', value: 750, reference: 775 },
            { label: 'Fev 26', value: 690, reference: 775 },
            { label: 'Mar 26', value: 810, reference: 775 },
            { label: 'Abr 26', value: 740, reference: 775 },
            { label: 'Mai 26', value: 920, reference: 775 },
            { label: 'Jun 26', value: 880, reference: 775 },
            { label: 'Jul 26', value: 760, reference: 775 },
            { label: 'Ago 26', value: 850, reference: 775 },
          ],
        },
      },
    ],
  },
  {
    label: 'Ligeiros',
    items: [
      {
        id: '236-542-008',
        driver: 'Pedro Torrezão',
        brand: 'Seat',
        model: 'Ateca',
        plate: '51-VM-88',
        status: 'A caminho',
        online: true,
        vin: 'VSSZZZ5FZM6031902',
        registered: '03/03/2021',
        nextInspection: '03/03/2027',
        insurer: 'Tranquilidade',
        insuranceRenewal: '20/11/2026',
        odometer: '68 450 km',
        maintenances: [
          { date: '20/09/2026', odometer: '68 320 km', workshop: 'Pneus & Alinhamento Pro', description: 'Alinhamento de rodas', cost: '120,00', type: 'preventiva' },
          { date: '05/09/2026', odometer: '68 150 km', workshop: 'Auto Serviço Central', description: 'Substituição de pastilhas de travão', cost: '195,00', type: 'corretiva' },
        ],
        stats: {
          kmsLastMonth: 1450,
          avgConsumption: 6.8,
          avgKmPerDay: 48.3,
          maintenanceCostLastMonth: 315,
          monthlyKms: [
            { label: 'Set 25', value: 1380, reference: 1450 },
            { label: 'Out 25', value: 1520, reference: 1450 },
            { label: 'Nov 25', value: 1290, reference: 1450 },
            { label: 'Dez 25', value: 1100, reference: 1450 },
            { label: 'Jan 26', value: 1450, reference: 1450 },
            { label: 'Fev 26', value: 1320, reference: 1450 },
            { label: 'Mar 26', value: 1680, reference: 1450 },
            { label: 'Abr 26', value: 1580, reference: 1450 },
            { label: 'Mai 26', value: 1750, reference: 1450 },
            { label: 'Jun 26', value: 1620, reference: 1450 },
            { label: 'Jul 26', value: 1480, reference: 1450 },
            { label: 'Ago 26', value: 1450, reference: 1450 },
          ],
        },
      },
      {
        id: '236-542-009',
        driver: 'Pedro Torrezão',
        brand: 'Smart',
        model: 'ForTwo',
        plate: '83-ZG-44',
        status: 'A caminho',
        online: true,
        vin: 'WME4513911K447203',
        registered: '22/09/2012',
        nextInspection: '22/09/2026',
        insurer: 'Ageas',
        insuranceRenewal: '05/02/2027',
        odometer: '112 300 km',
        maintenances: [
          { date: '10/09/2026', odometer: '112 250 km', workshop: 'Smart Care Center', description: 'Inspeção geral', cost: '65,00', type: 'preventiva' },
          { date: '15/08/2026', odometer: '112 080 km', workshop: 'Clima Auto', description: 'Limpeza do sistema de ar', cost: '40,00', type: 'preventiva' },
        ],
        stats: {
          kmsLastMonth: 620,
          avgConsumption: 5.1,
          avgKmPerDay: 20.7,
          maintenanceCostLastMonth: 105,
          monthlyKms: [
            { label: 'Set 25', value: 580, reference: 560 },
            { label: 'Out 25', value: 620, reference: 560 },
            { label: 'Nov 25', value: 490, reference: 560 },
            { label: 'Dez 25', value: 420, reference: 560 },
            { label: 'Jan 26', value: 550, reference: 560 },
            { label: 'Fev 26', value: 480, reference: 560 },
            { label: 'Mar 26', value: 640, reference: 560 },
            { label: 'Abr 26', value: 510, reference: 560 },
            { label: 'Mai 26', value: 680, reference: 560 },
            { label: 'Jun 26', value: 590, reference: 560 },
            { label: 'Jul 26', value: 560, reference: 560 },
            { label: 'Ago 26', value: 620, reference: 560 },
          ],
        },
      },
    ],
  },
]

const query = ref('')
const selectedId = ref('236-542-010')

const allVehicles = computed(() => groups.flatMap(group => group.items))
const selected = computed(() => allVehicles.value.find(v => v.id === selectedId.value) ?? allVehicles.value[0])

function slugify(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '')
}

function fullName(vehicle: Vehicle) {
  return `${vehicle.brand} ${vehicle.model}`
}

// As imagens saem do nome, para acrescentar um veiculo ser so pousar os
// ficheiros com o nome certo. Em falta, mostra-se um marcador de posicao.
function photoFor(vehicle: Vehicle) {
  return `/vehicles/${slugify(fullName(vehicle))}.png`
}

function logoFor(vehicle: Vehicle) {
  return `/brands/${slugify(vehicle.brand)}.png`
}

const photo = computed(() => photoFor(selected.value))

const visibleGroups = computed(() => {
  const term = query.value.trim().toLowerCase()
  if (!term) { return groups }
  return groups
    .map(group => ({
      ...group,
      items: group.items.filter(v =>
        fullName(v).toLowerCase().includes(term) || v.plate.toLowerCase().includes(term),
      ),
    }))
    .filter(group => group.items.length > 0)
})

function formatConsumption(value: number) {
  return `${value.toFixed(1)} l/100km`
}

function formatKms(value: number) {
  return value.toLocaleString('pt-PT') + ' km'
}

function formatCost(value: number) {
  return value.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })
}

</script>

<template>
  <ZSidePanel>
    <div class="side__search">
      <ZSearchInput v-model="query" placeholder="Procurar..." />
    </div>

    <div class="side__list">
      <ZEntityGroup v-for="group in visibleGroups" :key="group.label" :label="group.label">
        <ZEntityItem
          v-for="vehicle in group.items"
          :key="vehicle.id"
          :title="fullName(vehicle)"
          :subtitle="vehicle.plate"
          :image="logoFor(vehicle)"
          :status="vehicle.status"
          :alert="vehicle.alert"
          :selected="vehicle.id === selectedId"
          @click="selectedId = vehicle.id"
        />
      </ZEntityGroup>

      <p v-if="visibleGroups.length === 0" class="side__empty">
        Nenhum veículo corresponde a “{{ query }}”.
      </p>
    </div>

    <div class="side__footer">
      <ZButton variant="primary">+ Adicionar veículo</ZButton>
    </div>
  </ZSidePanel>

  <ZWorkspace>
    <ZPanel>
      <div class="head">
        <ZAvatar :name="selected.driver" size="lg" :online="selected.online" />
        <div>
          <p class="head__name">{{ selected.driver }}</p>
          <p class="head__id">ID: {{ selected.id }}</p>
        </div>
      </div>

      <div class="hero">
        <div class="hero__facts">
          <h2 class="hero__model">{{ fullName(selected) }}</h2>

          <ZStatGroup :columns="2">
            <ZStat class="facts__wide" label="VIN" :value="selected.vin" />
            <ZStat label="Data de matrícula" :value="selected.registered" />
            <ZStat label="Quilómetros" :value="selected.odometer" />
            <ZStat label="Seguradora" :value="selected.insurer" />
            <ZStat label="Renovação do seguro" :value="selected.insuranceRenewal" />
            <ZStat label="Próxima inspeção" :value="selected.nextInspection" />
          </ZStatGroup>

          <div class="hero__plate">
            <LicensePlate :value="selected.plate" />
            <a class="hero__docs" href="#documentos">Documentos</a>
          </div>
        </div>

        <VehiclePhoto :src="photo" :alt="fullName(selected)" />
      </div>
    </ZPanel>

    <div class="columns">
      <ZPanel title="Manutenção">
        <template #action>
          <ZButton size="sm">+ Adicionar</ZButton>
        </template>

        <div class="maintenance__timeline">
          <div v-for="(entry, index) in selected.maintenances" :key="entry.date" class="timeline__event">
            <div class="timeline__marker">
              <div class="timeline__dot" :class="`timeline__dot--${entry.type}`" />
            </div>
            <div class="timeline__content">
              <div class="timeline__header">
                <p class="timeline__date">{{ entry.date }}</p>
                <span class="timeline__badge" :class="`timeline__badge--${entry.type}`">{{ entry.type }}</span>
              </div>
              <p class="timeline__title">{{ entry.description }}</p>
              <div class="timeline__details">
                <div class="timeline__detail">
                  <span class="timeline__label">Oficina</span>
                  <p class="timeline__value">{{ entry.workshop }}</p>
                </div>
                <div class="timeline__detail">
                  <span class="timeline__label">Quilómetros</span>
                  <p class="timeline__value">{{ entry.odometer }}</p>
                </div>
                <div class="timeline__detail">
                  <span class="timeline__label">Custo</span>
                  <p class="timeline__value">{{ entry.cost }} €</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </ZPanel>

      <ZPanel title="Estatísticas do carro">
        <ZStatGroup :columns="2">
          <ZStat label="Quilómetros (últimos 30 dias)" :value="formatKms(selected.stats.kmsLastMonth)" />
          <ZStat label="Média/dia" :value="selected.stats.avgKmPerDay.toFixed(1) + ' km'" />
          <ZStat label="Consumo médio" :value="formatConsumption(selected.stats.avgConsumption)" />
          <ZStat label="Custos manutenção (últimos 30 dias)" :value="formatCost(selected.stats.maintenanceCostLastMonth)" />
        </ZStatGroup>

        <div class="stats__chart">
          <ZSectionLabel>Quilómetros por mês</ZSectionLabel>
          <ZBarChart
            :data="selected.stats.monthlyKms"
            value-label="Quilómetros"
            reference-label=""
            :show-reference="false"
          />
        </div>
      </ZPanel>
    </div>
  </ZWorkspace>
</template>

<style scoped>
.side__search {
  border-bottom: var(--z-border-subtle);
}

.side__list {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding-bottom: var(--z-space-2);
}

.side__empty {
  padding: var(--z-space-4);
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.side__footer {
  display: grid;
  padding: var(--z-space-3);
  border-top: var(--z-border-subtle);
}

.head {
  display: flex;
  align-items: center;
  gap: var(--z-space-3);
}

.head__name {
  margin: 0;
  font-weight: 600;
}

.head__id {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-6);
  flex-wrap: wrap;
  margin-top: var(--z-space-6);
  padding: var(--z-space-6);
  background: var(--z-color-surface);
  border-radius: var(--z-radius-lg);
}

.hero__facts {
  flex: 1 1 320px;
  min-width: 0;
}

/* O VIN tem 17 caracteres seguidos: numa coluna estreita nao ha onde
   partir e transbordava para cima do campo ao lado. */
.facts__wide {
  grid-column: 1 / -1;
}

.hero__model {
  margin: 0 0 var(--z-space-6);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
}

.hero__plate {
  display: flex;
  align-items: center;
  gap: var(--z-space-6);
  margin-top: var(--z-space-6);
}

.hero__docs,
.link {
  color: var(--z-color-text-muted);
  font-size: var(--z-font-size-sm);
  text-decoration-style: dashed;
  text-underline-offset: 3px;
}

.columns {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--z-space-3);
  align-items: start;
}

.maintenance__timeline {
  display: flex;
  flex-direction: column;
  gap: 0;
  margin: 0;
  padding: 0;
  list-style: none;
  position: relative;
}

/* Linha vertical central */
.maintenance__timeline::before {
  content: '';
  position: absolute;
  left: 15px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--z-color-border);
}

.timeline__event {
  display: flex;
  gap: var(--z-space-3);
  margin-bottom: var(--z-space-4);
  position: relative;
  padding-left: var(--z-space-8);
}

.timeline__marker {
  position: absolute;
  left: 0;
  top: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
}

.timeline__dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: var(--z-color-surface);
  border: 3px solid var(--z-color-border);
}

.timeline__dot--preventiva {
  background: var(--z-series-1);
  border-color: var(--z-series-1);
}

.timeline__dot--corretiva {
  background: var(--z-series-4);
  border-color: var(--z-series-4);
}

.timeline__content {
  flex: 1;
  padding: var(--z-space-3);
  background: var(--z-color-surface);
  border-radius: var(--z-radius);
}

.timeline__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-2);
  margin-bottom: var(--z-space-1);
}

.timeline__date {
  margin: 0;
  font-size: var(--z-font-size-sm);
  font-weight: 600;
}

.timeline__badge {
  padding: 2px 8px;
  font-size: var(--z-font-size-xs);
  font-weight: 600;
  border-radius: var(--z-radius-sm);
  text-transform: capitalize;
  white-space: nowrap;
}

.timeline__badge--preventiva {
  background: var(--z-series-1);
  color: #fff;
}

.timeline__badge--corretiva {
  background: var(--z-series-4);
  color: #fff;
}

.timeline__title {
  margin: 0 0 var(--z-space-2);
  font-size: var(--z-font-size-sm);
  font-weight: 600;
}

.timeline__details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: var(--z-space-3);
}

.timeline__detail {
  display: flex;
  flex-direction: column;
}

.timeline__label {
  font-size: var(--z-font-size-xs);
  color: var(--z-color-text-muted);
  margin-bottom: 2px;
}

.timeline__value {
  margin: 0;
  font-size: var(--z-font-size-sm);
  font-weight: 600;
  color: var(--z-color-text);
}

.stats__chart {
  margin-top: var(--z-space-8);
}
</style>
