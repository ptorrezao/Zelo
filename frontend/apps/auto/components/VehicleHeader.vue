<script setup lang="ts">
import { reactive, ref } from 'vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import AddVehicleSheet from './AddVehicleSheet.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, fullName, logoFor } = useVehicles()

// Se o logo da marca nao existir/carregar, cai para a inicial da marca.
const logoErrors = reactive<Record<string, boolean>>({})
const markLogoError = (vehicleId: string) => {
  logoErrors[vehicleId] = true
}

const isEditOpen = ref(false)
</script>

<template>
  <div class="flex items-center gap-4 rounded-lg border border-border bg-card p-6 shadow-sm">
    <div class="flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-full bg-secondary text-base font-semibold">
      <img
        v-if="selected && !logoErrors[selected.id]"
        :src="logoFor(selected)"
        :alt="selected.brand"
        class="h-full w-full object-contain p-2"
        @error="markLogoError(selected!.id)"
      />
      <span v-else>{{ selected?.brand?.substring(0, 1) }}</span>
    </div>
    <div class="flex-1">
      <h1 class="text-xl font-semibold">{{ fullName(selected) }}</h1>
      <p class="text-sm text-muted-foreground">{{ selected?.plate }}</p>
    </div>
    <Button variant="outline" size="sm" @click="isEditOpen = true">Editar</Button>
  </div>

  <AddVehicleSheet v-model:open="isEditOpen" :vehicle-id="selected?.id" />
</template>
