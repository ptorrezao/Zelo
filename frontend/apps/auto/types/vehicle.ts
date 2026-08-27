export interface MaintenanceItem {
  description: string
  price: string
  serialNumber?: string
}

export interface MaintenanceInvoice {
  number: string
  date: string
  url: string
}

export interface Maintenance {
  id: string
  date: string
  odometer: string
  workshop: string
  description: string
  cost: string
  type: 'preventiva' | 'corretiva' | 'inspecao'
  items?: MaintenanceItem[]
  invoice?: MaintenanceInvoice
}

export interface VehicleStats {
  kmsLastMonth: number
  avgConsumption: number
  avgKmPerDay: number
  maintenanceCostLastMonth: number
  monthlyKms: { label: string, value: number, reference: number }[]
}

export interface VehicleDocument {
  id: string
  name: string
  category: 'Seguro' | 'Manutenção' | 'Inspeção' | 'Registo' | 'Fatura'
  type: 'pdf' | 'imagem'
  date: string
  size: string
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
  iucDueDate: string
  odometer: string
  maintenances: Maintenance[]
  documents: VehicleDocument[]
  stats: VehicleStats
  alert?: boolean
  online?: boolean
}

export interface VehicleGroup {
  label: string
  items: Vehicle[]
}
