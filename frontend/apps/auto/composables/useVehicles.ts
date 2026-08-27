import { computed, ref } from 'vue'
import type { Vehicle, VehicleGroup } from '../types/vehicle'

// TODO: Replace with API call to fetch vehicles
const vehicleGroups: VehicleGroup[] = [
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
          { date: '28/08/2026', odometer: '24 420 km', workshop: 'Manutenção Total', description: 'Inspeção periódica', cost: '45,00', type: 'inspecao' },
          { date: '15/06/2026', odometer: '23 980 km', workshop: 'Auto Serviço Silva', description: 'Reparação da correia', cost: '280,00', type: 'corretiva' },
          { date: '02/03/2026', odometer: '22 100 km', workshop: 'Manutenção Total', description: 'Substituição de pneus', cost: '340,00', type: 'preventiva' },
          { date: '18/11/2025', odometer: '20 340 km', workshop: 'Auto Serviço Silva', description: 'Troca de pastilhas de travão', cost: '95,00', type: 'corretiva' },
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
          { date: '22/06/2026', odometer: '66 800 km', workshop: 'Pneus & Alinhamento Pro', description: 'Inspeção periódica', cost: '50,00', type: 'inspecao' },
          { date: '10/02/2026', odometer: '64 200 km', workshop: 'Auto Serviço Central', description: 'Troca de óleo e filtros', cost: '110,00', type: 'preventiva' },
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
          { date: '10/09/2026', odometer: '112 250 km', workshop: 'Smart Care Center', description: 'Inspeção periódica', cost: '65,00', type: 'inspecao' },
          { date: '15/08/2026', odometer: '112 080 km', workshop: 'Clima Auto', description: 'Limpeza do sistema de ar', cost: '40,00', type: 'preventiva' },
          { date: '30/04/2026', odometer: '109 500 km', workshop: 'Smart Care Center', description: 'Reparação do sistema elétrico', cost: '210,00', type: 'corretiva' },
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

// Estado partilhado a nivel de modulo: todas as chamadas a useVehicles()
// devolvem as mesmas refs, para a sidebar e o conteudo principal
// (componentes diferentes) ficarem sincronizados na mesma selecao.
const groups = ref(vehicleGroups)
const query = ref('')
const selectedId = ref('236-542-010')

export function useVehicles() {
  const allVehicles = computed(() => groups.value.flatMap(group => group.items))
  const selected = computed(() => allVehicles.value.find(v => v.id === selectedId.value) ?? allVehicles.value[0])
  const photo = computed(() => photoFor(selected.value))

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

  return {
    groups,
    query,
    selectedId,
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
  }
}
