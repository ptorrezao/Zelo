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
import Combobox from '@zelo/ui/components/ui/Combobox.vue'
import DatePicker from '@zelo/ui/components/ui/DatePicker.vue'
import { useVehicles } from '../composables/useVehicles'
import { useVehicleCatalog } from '../composables/useVehicleCatalog'
import { toIso } from '../utils/vehicleFormat'

// Quando vehicleId e passado, o sheet abre em modo de edicao: os campos
// vem pre-preenchidos com os dados desse veiculo e o submit atualiza-o
// em vez de criar um novo.
const props = defineProps<{ vehicleId?: string }>()

const open = defineModel<boolean>('open', { default: false })

const { allVehicles, addVehicle, updateVehicle, categoryOf, selectedId } = useVehicles()
const { catalog } = useVehicleCatalog()

const isEditMode = computed(() => !!props.vehicleId)

const category = ref<'Motociclos' | 'Ligeiros'>('Ligeiros')
const brand = ref('')
const model = ref('')
const plate = ref('')
const vin = ref('')
const color = ref('')
const odometer = ref('')
const registered = ref('')
const nextInspection = ref('')
const insurer = ref('')
const insurancePolicyNumber = ref('')
const insurancePeriodStart = ref('')
const insurancePeriodEnd = ref('')
const insurancePremium = ref('')
const iucDueDate = ref('')
const error = ref('')

const categoryOptions = [
  { value: 'Ligeiros', label: 'Ligeiro' },
  { value: 'Motociclos', label: 'Motociclo' },
]

// Marca/Modelo/Cor sao texto livre com sugestoes (datalist), nao uma
// lista fechada - o catalogo (vindo do backend, ver useVehicleCatalog)
// nunca vai ter todas as marcas e modelos possiveis, e antes bloqueava o
// utilizador se a dele nao estivesse listada. Servem so para sugerir, o
// valor final pode ser qualquer texto.
// O catalogo vem contextualizado por categoria (ver useVehicleCatalog) -
// sem isto, escolher "Motociclo" continuava a sugerir marcas/modelos de
// carro (e vice-versa) da mesma marca, ex. BMW misturava a Serie 1 com a
// R1250GS.
const brandsForCategory = computed(() => catalog.value[category.value] ?? {})
const brandSuggestions = computed(() => Object.keys(brandsForCategory.value))
const modelSuggestions = computed(() => brandsForCategory.value[brand.value] ?? [])

// So quando o utilizador troca a categoria a mao - marca/modelo de uma
// categoria raramente fazem sentido na outra. Um watch(category, ...)
// dispararia tambem quando reset()/loadFromVehicle() atribuem
// category.value de proposito (ao abrir o sheet), apagando o que acabaram
// de preencher - por isso o handler fica so aqui, ligado ao evento do
// Select, e reset()/loadFromVehicle() continuam a atribuir category.value
// diretamente sem passar por ele.
function handleCategoryChange(value: string) {
  category.value = value as typeof category.value
  brand.value = ''
  model.value = ''
}
const COLOR_SUGGESTIONS = [
  'Branco', 'Preto', 'Cinzento', 'Prata', 'Azul', 'Vermelho',
  'Verde', 'Amarelo', 'Castanho', 'Bege', 'Laranja', 'Roxo',
]

function reset() {
  category.value = 'Ligeiros'
  brand.value = ''
  model.value = ''
  plate.value = ''
  vin.value = ''
  color.value = ''
  odometer.value = ''
  registered.value = ''
  nextInspection.value = ''
  insurer.value = ''
  insurancePolicyNumber.value = ''
  insurancePeriodStart.value = ''
  insurancePeriodEnd.value = ''
  insurancePremium.value = ''
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
  color.value = vehicle.color === '—' ? '' : vehicle.color
  odometer.value = vehicle.odometer
  registered.value = vehicle.registered === '—' ? '' : vehicle.registered
  nextInspection.value = vehicle.nextInspection === '—' ? '' : vehicle.nextInspection
  insurer.value = vehicle.insurer === '—' ? '' : vehicle.insurer
  insurancePolicyNumber.value = vehicle.insurancePolicyNumber === '—' ? '' : vehicle.insurancePolicyNumber
  insurancePeriodStart.value = vehicle.insurancePeriodStart === '—' ? '' : vehicle.insurancePeriodStart
  insurancePeriodEnd.value = vehicle.insurancePeriodEnd === '—' ? '' : vehicle.insurancePeriodEnd
  insurancePremium.value = vehicle.insurancePremium === '—' ? '' : vehicle.insurancePremium
  iucDueDate.value = vehicle.iucDueDate === '—' ? '' : vehicle.iucDueDate
}

// Preenche o formulario quando o sheet abre: com os dados do veiculo em
// modo de edicao, ou em branco para adicionar um novo.
watch(open, (isOpen) => {
  if (!isOpen) return
  error.value = ''
  if (props.vehicleId) {
    loadFromVehicle(props.vehicleId)
  } else {
    reset()
  }
})

const isSubmitting = ref(false)

// Espelha as mesmas regras do backend (VehicleValidation) para dar
// feedback imediato sem esperar por um pedido - o backend continua a
// ser a fonte de verdade (chamavel via MCP, sem passar por este
// formulario) e o catch em handleSubmit mostra o que vier de la (ex.
// matricula duplicada, que so a BD sabe dizer).
function validate(): string | null {
  if (!brand.value.trim()) return 'Marca é obrigatória.'
  if (!model.value.trim()) return 'Modelo é obrigatório.'
  if (!plate.value.trim()) return 'Matrícula é obrigatória.'
  if (!vin.value.trim()) return 'VIN é obrigatório.'
  if (!color.value.trim()) return 'Cor é obrigatória.'
  if (!odometer.value.trim()) return 'Quilometragem é obrigatória.'
  if (!toIso(registered.value)) return 'Data de matrícula é obrigatória.'

  const periodStart = toIso(insurancePeriodStart.value)
  const periodEnd = toIso(insurancePeriodEnd.value)
  if (periodStart && periodEnd && periodEnd < periodStart) {
    return 'Fim do período de seguro não pode ser antes do início.'
  }

  return null
}

async function handleSubmit() {
  const validationError = validate()
  if (validationError) {
    error.value = validationError
    return
  }

  const input = {
    category: category.value,
    brand: brand.value,
    model: model.value,
    plate: plate.value,
    vin: vin.value,
    color: color.value,
    odometer: odometer.value,
    registered: registered.value,
    nextInspection: nextInspection.value,
    insurer: insurer.value,
    insurancePolicyNumber: insurancePolicyNumber.value,
    insurancePeriodStart: insurancePeriodStart.value,
    insurancePeriodEnd: insurancePeriodEnd.value,
    insurancePremium: insurancePremium.value,
    iucDueDate: iucDueDate.value,
  }

  error.value = ''
  isSubmitting.value = true
  try {
    if (isEditMode.value && props.vehicleId) {
      await updateVehicle(props.vehicleId, input)
    } else {
      const vehicle = await addVehicle(input)
      selectedId.value = vehicle.id
    }
    open.value = false
  } catch (err) {
    // Nunca fecha o sheet num erro - antes disto, updateVehicle falhado
    // fechava na mesma (parecia ter funcionado) e addVehicle rebentava
    // com uma excecao nao tratada em vez de mostrar a mensagem do
    // backend (ex. matricula duplicada).
    error.value = err instanceof Error ? err.message : 'Não foi possível guardar o veículo.'
  } finally {
    isSubmitting.value = false
  }
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
        <div
          v-if="error"
          class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
        >
          {{ error }}
        </div>

        <div class="flex flex-col gap-2">
          <label class="text-sm font-medium">Categoria</label>
          <Select :model-value="category" :options="categoryOptions" @update:model-value="handleCategoryChange" />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="brand" class="text-sm font-medium">Marca</label>
            <Combobox id="brand" v-model="brand" :options="brandSuggestions" placeholder="Marca" />
          </div>
          <div class="flex flex-col gap-2">
            <label for="model" class="text-sm font-medium">Modelo</label>
            <Combobox id="model" v-model="model" :options="modelSuggestions" placeholder="Modelo" />
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <label for="plate" class="text-sm font-medium">Matrícula</label>
          <Input id="plate" v-model="plate" placeholder="Ex.: AA-00-AA" required />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="vin" class="text-sm font-medium">VIN</label>
            <Input id="vin" v-model="vin" placeholder="Número de chassis" />
          </div>
          <div class="flex flex-col gap-2">
            <label for="color" class="text-sm font-medium">Cor</label>
            <Combobox id="color" v-model="color" :options="COLOR_SUGGESTIONS" placeholder="Cor" />
          </div>
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

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="insurer" class="text-sm font-medium">Seguradora</label>
            <Input id="insurer" v-model="insurer" placeholder="Ex.: Fidelidade" />
          </div>
          <div class="flex flex-col gap-2">
            <label for="insurance-policy-number" class="text-sm font-medium">Nº da apólice</label>
            <Input id="insurance-policy-number" v-model="insurancePolicyNumber" placeholder="Ex.: AP-12345" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Início do período</label>
            <DatePicker v-model="insurancePeriodStart" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Fim do período</label>
            <DatePicker v-model="insurancePeriodEnd" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="insurance-premium" class="text-sm font-medium">Valor do prémio</label>
            <Input id="insurance-premium" v-model="insurancePremium" placeholder="Ex.: 350,00" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Data do IUC</label>
            <DatePicker v-model="iucDueDate" />
          </div>
        </div>
      </form>

      <SheetFooter>
        <Button variant="outline" @click="close">Cancelar</Button>
        <Button :disabled="isSubmitting" @click="handleSubmit">{{ isEditMode ? 'Guardar' : 'Adicionar' }}</Button>
      </SheetFooter>
    </template>
  </Sheet>
</template>
