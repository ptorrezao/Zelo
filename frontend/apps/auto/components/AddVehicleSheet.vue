<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Sheet from '@zelo/ui/components/ui/Sheet.vue'
import SheetHeader from '@zelo/ui/components/ui/SheetHeader.vue'
import SheetTitle from '@zelo/ui/components/ui/SheetTitle.vue'
import SheetDescription from '@zelo/ui/components/ui/SheetDescription.vue'
import SheetFooter from '@zelo/ui/components/ui/SheetFooter.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Input from '@zelo/ui/components/ui/Input.vue'
import Select from '@zelo/ui/components/ui/Select.vue'
import DatePicker from '@zelo/ui/components/ui/DatePicker.vue'
import { useVehicles } from '../composables/useVehicles'
import { VEHICLE_BRANDS, VEHICLE_CATALOG } from '../data/vehicleCatalog'

// Quando vehicleId e passado, o sheet abre em modo de edicao: os campos
// vem pre-preenchidos com os dados desse veiculo e o submit atualiza-o
// em vez de criar um novo.
const props = defineProps<{ vehicleId?: string }>()

const open = defineModel<boolean>('open', { default: false })

const { allVehicles, addVehicle, updateVehicle, categoryOf, selectedId } = useVehicles()

const isEditMode = computed(() => !!props.vehicleId)

const category = ref<'Motociclos' | 'Ligeiros'>('Ligeiros')
const brand = ref('')
const model = ref('')
const plate = ref('')
const vin = ref('')
const odometer = ref('')
const registered = ref('')
const nextInspection = ref('')
const insurer = ref('')
const insuranceRenewal = ref('')
const iucDueDate = ref('')

const categoryOptions = [
  { value: 'Ligeiros', label: 'Ligeiro' },
  { value: 'Motociclos', label: 'Motociclo' },
]
const brandOptions = computed(() => VEHICLE_BRANDS.map(b => ({ value: b, label: b })))
const modelOptions = computed(() => (VEHICLE_CATALOG[brand.value] ?? []).map(m => ({ value: m, label: m })))

// Trocar de marca limpa o modelo se ja nao fizer parte do catalogo dela.
watch(brand, () => {
  if (!VEHICLE_CATALOG[brand.value]?.includes(model.value)) {
    model.value = ''
  }
})

function reset() {
  category.value = 'Ligeiros'
  brand.value = ''
  model.value = ''
  plate.value = ''
  vin.value = ''
  odometer.value = ''
  registered.value = ''
  nextInspection.value = ''
  insurer.value = ''
  insuranceRenewal.value = ''
  iucDueDate.value = ''
}

function loadFromVehicle(vehicleId: string) {
  const vehicle = allVehicles.value.find(v => v.id === vehicleId)
  if (!vehicle) return
  category.value = categoryOf(vehicleId) ?? 'Ligeiros'
  brand.value = vehicle.brand
  model.value = vehicle.model
  plate.value = vehicle.plate
  vin.value = vehicle.vin === '—' ? '' : vehicle.vin
  odometer.value = vehicle.odometer
  registered.value = vehicle.registered === '—' ? '' : vehicle.registered
  nextInspection.value = vehicle.nextInspection === '—' ? '' : vehicle.nextInspection
  insurer.value = vehicle.insurer === '—' ? '' : vehicle.insurer
  insuranceRenewal.value = vehicle.insuranceRenewal === '—' ? '' : vehicle.insuranceRenewal
  iucDueDate.value = vehicle.iucDueDate === '—' ? '' : vehicle.iucDueDate
}

// Preenche o formulario quando o sheet abre: com os dados do veiculo em
// modo de edicao, ou em branco para adicionar um novo.
watch(open, (isOpen) => {
  if (!isOpen) return
  if (props.vehicleId) {
    loadFromVehicle(props.vehicleId)
  } else {
    reset()
  }
})

function handleSubmit() {
  if (!brand.value || !model.value || !plate.value) return

  const input = {
    category: category.value,
    brand: brand.value,
    model: model.value,
    plate: plate.value,
    vin: vin.value,
    odometer: odometer.value,
    registered: registered.value,
    nextInspection: nextInspection.value,
    insurer: insurer.value,
    insuranceRenewal: insuranceRenewal.value,
    iucDueDate: iucDueDate.value,
  }

  if (isEditMode.value && props.vehicleId) {
    updateVehicle(props.vehicleId, input)
  } else {
    const vehicle = addVehicle(input)
    selectedId.value = vehicle.id
  }

  open.value = false
}
</script>

<template>
  <Sheet v-model:open="open">
    <template #default="{ close }">
      <SheetHeader>
        <SheetTitle>{{ isEditMode ? 'Editar veículo' : 'Adicionar veículo' }}</SheetTitle>
        <SheetDescription>
          {{ isEditMode ? 'Atualize a informação deste veículo.' : 'Registe um novo veículo na sua frota.' }}
        </SheetDescription>
      </SheetHeader>

      <form class="flex flex-1 flex-col gap-4 overflow-y-auto pr-1" @submit.prevent="handleSubmit">
        <div class="flex flex-col gap-2">
          <label class="text-sm font-medium">Categoria</label>
          <Select v-model="category" :options="categoryOptions" />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Marca</label>
            <Select v-model="brand" :options="brandOptions" placeholder="Marca" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Modelo</label>
            <Select v-model="model" :options="modelOptions" placeholder="Modelo" :disabled="!brand" />
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <label for="plate" class="text-sm font-medium">Matrícula</label>
          <Input id="plate" v-model="plate" placeholder="Ex.: AA-00-AA" required />
        </div>

        <div class="flex flex-col gap-2">
          <label for="vin" class="text-sm font-medium">VIN</label>
          <Input id="vin" v-model="vin" placeholder="Número de chassis" />
        </div>

        <div class="flex flex-col gap-2">
          <label for="odometer" class="text-sm font-medium">Quilómetros</label>
          <Input id="odometer" v-model="odometer" placeholder="Ex.: 24 780 km" />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Data de matrícula</label>
            <DatePicker v-model="registered" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Próxima inspeção</label>
            <DatePicker v-model="nextInspection" />
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <label for="insurer" class="text-sm font-medium">Seguradora</label>
          <Input id="insurer" v-model="insurer" placeholder="Ex.: Fidelidade" />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Renovação do seguro</label>
            <DatePicker v-model="insuranceRenewal" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Data do IUC</label>
            <DatePicker v-model="iucDueDate" />
          </div>
        </div>
      </form>

      <SheetFooter>
        <Button variant="outline" @click="close">Cancelar</Button>
        <Button @click="handleSubmit">{{ isEditMode ? 'Guardar' : 'Adicionar' }}</Button>
      </SheetFooter>
    </template>
  </Sheet>
</template>
