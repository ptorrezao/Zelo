<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Sheet from '@zelo/ui/components/ui/Sheet.vue'
import SheetHeader from '@zelo/ui/components/ui/SheetHeader.vue'
import SheetTitle from '@zelo/ui/components/ui/SheetTitle.vue'
import SheetDescription from '@zelo/ui/components/ui/SheetDescription.vue'
import SheetFooter from '@zelo/ui/components/ui/SheetFooter.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Input from '@zelo/ui/components/ui/Input.vue'
import { useVehicles } from '../composables/useVehicles'

const open = defineModel<boolean>('open', { default: false })

const router = useRouter()
const { selected, addMaintenance } = useVehicles()

const date = ref('')
const type = ref<'preventiva' | 'corretiva' | 'inspecao'>('preventiva')
const workshop = ref('')
const description = ref('')
const cost = ref('')
const odometer = ref('')

function reset() {
  date.value = ''
  type.value = 'preventiva'
  workshop.value = ''
  description.value = ''
  cost.value = ''
  odometer.value = ''
}

const isSubmitting = ref(false)

async function handleSubmit() {
  if (!selected.value) return
  if (!date.value || !workshop.value || !description.value || !cost.value || !odometer.value) return

  isSubmitting.value = true
  try {
    const maintenance = await addMaintenance(selected.value.id, {
      date: date.value,
      type: type.value,
      workshop: workshop.value,
      description: description.value,
      cost: cost.value,
      odometer: odometer.value,
    })

    reset()
    open.value = false

    if (maintenance) {
      router.push(`/manutencao/${maintenance.id}`)
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <Sheet v-model:open="open">
    <template #default="{ close }">
      <SheetHeader>
        <SheetTitle>Adicionar manutenção</SheetTitle>
        <SheetDescription>Registe uma nova entrada para {{ selected ? `${selected.brand} ${selected.model}` : 'este veículo' }}.</SheetDescription>
      </SheetHeader>

      <form class="flex flex-1 flex-col gap-4 overflow-y-auto" @submit.prevent="handleSubmit">
        <div class="flex flex-col gap-2">
          <label for="mnt-date" class="text-sm font-medium">Data</label>
          <Input id="mnt-date" v-model="date" placeholder="DD/MM/AAAA" required />
        </div>

        <div class="flex flex-col gap-2">
          <label for="mnt-type" class="text-sm font-medium">Tipo</label>
          <select
            id="mnt-type"
            v-model="type"
            class="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <option value="preventiva">Preventiva</option>
            <option value="corretiva">Corretiva</option>
            <option value="inspecao">Inspeção</option>
          </select>
        </div>

        <div class="flex flex-col gap-2">
          <label for="mnt-workshop" class="text-sm font-medium">Oficina</label>
          <Input id="mnt-workshop" v-model="workshop" placeholder="Ex.: Auto Serviço Silva" required />
        </div>

        <div class="flex flex-col gap-2">
          <label for="mnt-description" class="text-sm font-medium">Descrição</label>
          <Input id="mnt-description" v-model="description" placeholder="Ex.: Troca de óleo e filtros" required />
        </div>

        <div class="flex flex-col gap-2">
          <label for="mnt-odometer" class="text-sm font-medium">Quilómetros</label>
          <Input id="mnt-odometer" v-model="odometer" placeholder="Ex.: 24 650 km" required />
        </div>

        <div class="flex flex-col gap-2">
          <label for="mnt-cost" class="text-sm font-medium">Custo (€)</label>
          <Input id="mnt-cost" v-model="cost" placeholder="Ex.: 85,00" required />
        </div>
      </form>

      <SheetFooter>
        <Button variant="outline" @click="close">Cancelar</Button>
        <Button :disabled="isSubmitting" @click="handleSubmit">Adicionar</Button>
      </SheetFooter>
    </template>
  </Sheet>
</template>
