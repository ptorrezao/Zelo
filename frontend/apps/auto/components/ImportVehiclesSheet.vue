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
import { useImportVehicles } from '../composables/useImportVehicles'
import { useVehicles } from '../composables/useVehicles'
import { fromIso } from '../utils/vehicleFormat'
import type { ImportCandidateVehicle, ImportHousehold, ImportPreviewItem, ImportConfirmResponse } from '../composables/useImportVehicles'

const open = defineModel<boolean>('open', { default: false })

const { listMyHouseholds, getMyEmail, previewImport, confirmImport } = useImportVehicles()
const { refresh: refreshVehicles } = useVehicles()

type Step = 'connect' | 'chooseOrigin' | 'review' | 'result'
const step = ref<Step>('connect')

const isLoading = ref(false)
const errorMessage = ref('')

// Households do utilizador atual, para escolher o destino - carregados ao
// abrir a sheet (nao ha garantia de so haver um, ao contrario do resto da
// app que assume DEFAULT_HOUSEHOLD_ID).
const myHouseholds = ref<ImportHousehold[]>([])
const destinationHouseholdId = ref('')

const baseUrl = ref('')
const email = ref('')
const password = ref('')

// Para avisar logo no formulario que nao pode importar de si mesmo - o
// backend recusa de qualquer forma, isto e so para nao deixar preencher
// tudo e password incluida so para descobrir no fim.
const myEmail = ref<string | null>(null)
const isSelfImport = computed(() =>
  !!myEmail.value && email.value.trim().toLowerCase() === myEmail.value.toLowerCase())

// Preenchido quando o utilizador de origem tem mais que um household -
// guardado entre a chamada inicial e a repeticao do preview depois de
// escolher.
const originHouseholds = ref<ImportHousehold[]>([])
const remoteHouseholdId = ref('')

const previewItems = ref<ImportPreviewItem[]>([])
const selectedPlates = ref<Set<string>>(new Set())

const result = ref<ImportConfirmResponse | null>(null)

function reset() {
  step.value = 'connect'
  errorMessage.value = ''
  baseUrl.value = ''
  email.value = ''
  password.value = ''
  originHouseholds.value = []
  remoteHouseholdId.value = ''
  previewItems.value = []
  selectedPlates.value = new Set()
  result.value = null
}

watch(open, async (isOpen) => {
  if (!isOpen) return
  reset()
  try {
    myHouseholds.value = await listMyHouseholds()
    destinationHouseholdId.value = myHouseholds.value[0]?.id ?? ''
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Não foi possível carregar os seus households.'
  }
  myEmail.value = await getMyEmail()
})

const householdOptions = ref<{ value: string, label: string }[]>([])
watch(myHouseholds, (households) => {
  householdOptions.value = households.map(h => ({ value: h.id, label: h.name }))
}, { immediate: true })

async function runPreview() {
  if (isSelfImport.value) return

  errorMessage.value = ''
  isLoading.value = true
  try {
    const response = await previewImport({
      destinationHouseholdId: destinationHouseholdId.value,
      baseUrl: baseUrl.value,
      email: email.value,
      password: password.value,
      remoteHouseholdId: remoteHouseholdId.value || undefined,
    })

    if (response.status === 'ChooseHousehold') {
      originHouseholds.value = response.households
      step.value = 'chooseOrigin'
      return
    }

    previewItems.value = response.vehicles
    selectedPlates.value = new Set(response.vehicles.filter(v => !v.alreadyExists).map(v => v.vehicle.plate))
    step.value = 'review'
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Não foi possível ligar ao ambiente de origem.'
  } finally {
    isLoading.value = false
  }
}

function chooseOrigin(householdId: string) {
  remoteHouseholdId.value = householdId
  void runPreview()
}

function toggleSelected(plate: string) {
  const next = new Set(selectedPlates.value)
  if (next.has(plate)) {
    next.delete(plate)
  } else {
    next.add(plate)
  }
  selectedPlates.value = next
}

async function runConfirm() {
  const chosen: ImportCandidateVehicle[] = previewItems.value
    .filter(item => selectedPlates.value.has(item.vehicle.plate))
    .map(item => item.vehicle)
  if (chosen.length === 0) return

  errorMessage.value = ''
  isLoading.value = true
  try {
    result.value = await confirmImport(destinationHouseholdId.value, chosen)
    step.value = 'result'
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Falha ao importar veículos.'
  } finally {
    isLoading.value = false
  }
}

function finish() {
  open.value = false
  void refreshVehicles()
}
</script>

<template>
  <Sheet v-model:open="open">
    <template #default="{ close }">
      <SheetHeader>
        <SheetTitle>Importar veículos</SheetTitle>
        <SheetDescription>
          Puxe veículos de outro utilizador, autenticando-se com as credenciais dele.
        </SheetDescription>
      </SheetHeader>

      <div class="flex flex-1 flex-col gap-4 overflow-y-auto pr-1">
        <p v-if="errorMessage" class="rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          {{ errorMessage }}
        </p>

        <form v-if="step === 'connect'" class="flex flex-col gap-4" @submit.prevent="runPreview">
          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Importar para</label>
            <Select v-model="destinationHouseholdId" :options="householdOptions" placeholder="Household de destino" />
          </div>

          <div class="flex flex-col gap-2">
            <label for="import-base-url" class="text-sm font-medium">URL do ambiente de origem</label>
            <Input id="import-base-url" v-model="baseUrl" placeholder="https://exemplo.zelo.app" required />
            <p class="text-xs text-muted-foreground">
              O site que essa pessoa usa normalmente no browser.
            </p>
          </div>

          <div class="flex flex-col gap-2">
            <label for="import-email" class="text-sm font-medium">Email</label>
            <Input id="import-email" v-model="email" type="email" placeholder="utilizador@exemplo.com" required />
            <p v-if="isSelfImport" class="text-xs text-destructive">
              Não pode importar veículos de si mesmo — indique as credenciais de outro utilizador.
            </p>
          </div>

          <div class="flex flex-col gap-2">
            <label for="import-password" class="text-sm font-medium">Palavra-passe</label>
            <Input id="import-password" v-model="password" type="password" required />
          </div>
        </form>

        <div v-else-if="step === 'chooseOrigin'" class="flex flex-col gap-2">
          <p class="text-sm text-muted-foreground">Este utilizador pertence a mais do que um household. Escolha de onde importar:</p>
          <Button
            v-for="household in originHouseholds"
            :key="household.id"
            type="button"
            variant="outline"
            class="justify-start"
            :disabled="isLoading"
            @click="chooseOrigin(household.id)"
          >
            {{ household.name }}
          </Button>
        </div>

        <div v-else-if="step === 'review'" class="flex flex-col gap-2">
          <p class="text-sm text-muted-foreground">
            {{ previewItems.length }} veículo(s) encontrado(s). Reveja e escolha o que importar.
          </p>
          <div
            v-for="item in previewItems"
            :key="item.vehicle.plate"
            class="flex items-center gap-3 rounded-md border border-border p-3"
          >
            <input
              type="checkbox"
              class="h-4 w-4"
              :checked="selectedPlates.has(item.vehicle.plate)"
              @change="toggleSelected(item.vehicle.plate)"
            />
            <div class="flex min-w-0 flex-1 flex-col">
              <span class="truncate text-sm font-medium">
                {{ item.vehicle.brand }} {{ item.vehicle.model }}<span v-if="item.vehicle.color"> · {{ item.vehicle.color }}</span>
              </span>
              <span class="text-xs text-muted-foreground">
                {{ item.vehicle.plate }} · {{ item.vehicle.driver ?? 'Sem condutor' }} · matriculado em {{ fromIso(item.vehicle.registered) }}
              </span>
            </div>
            <span
              v-if="item.alreadyExists"
              class="shrink-0 rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-800"
            >
              Já existe
            </span>
          </div>
        </div>

        <div v-else-if="step === 'result' && result" class="flex flex-col gap-2">
          <p class="text-sm font-medium">
            {{ result.importedCount }} importado(s), {{ result.skippedCount }} ignorado(s).
          </p>
          <div v-if="result.skippedCount > 0" class="flex flex-col gap-1">
            <p class="text-xs font-medium text-muted-foreground">Ignorados:</p>
            <p v-for="item in result.items.filter(i => !i.imported)" :key="item.plate" class="text-xs text-muted-foreground">
              {{ item.plate }} — {{ item.skipReason }}
            </p>
          </div>
        </div>
      </div>

      <SheetFooter>
        <template v-if="step === 'connect'">
          <Button variant="outline" @click="close">Cancelar</Button>
          <Button :disabled="isLoading || isSelfImport" @click="runPreview">{{ isLoading ? 'A ligar…' : 'Ligar e importar' }}</Button>
        </template>
        <template v-else-if="step === 'review'">
          <Button variant="outline" :disabled="isLoading" @click="step = 'connect'">Voltar</Button>
          <Button :disabled="isLoading || selectedPlates.size === 0" @click="runConfirm">
            {{ isLoading ? 'A importar…' : `Importar selecionados (${selectedPlates.size})` }}
          </Button>
        </template>
        <template v-else-if="step === 'result'">
          <Button @click="() => { finish(); close() }">Concluir</Button>
        </template>
      </SheetFooter>
    </template>
  </Sheet>
</template>
