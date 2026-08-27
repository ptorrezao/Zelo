export interface Maintenance {
  date: string
  odometer: string
  workshop: string
  description: string
  cost: string
  type: 'preventiva' | 'corretiva'
}

export interface VehicleStats {
  kmsLastMonth: number
  avgConsumption: number
  avgKmPerDay: number
  maintenanceCostLastMonth: number
  monthlyKms: { label: string, value: number, reference: number }[]
}

export interface Vehicle {
  id: string
  driver: string
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

export interface VehicleGroup {
  label: string
  items: Vehicle[]
}
