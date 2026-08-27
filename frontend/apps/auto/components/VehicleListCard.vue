<script setup lang="ts">
import { reactive } from 'vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import { useVehicles } from '../composables/useVehicles'

const { visibleGroups, selectedId, fullName, logoFor } = useVehicles()

// Todos os grupos comecam abertos; cada um pode ser fechado individualmente.
const openGroups = reactive<Record<string, boolean>>({})
const isOpen = (label: string) => openGroups[label] ?? true
const toggleGroup = (label: string) => {
  openGroups[label] = !isOpen(label)
}

// Se o logo da marca nao existir/carregar, cai para a inicial do veiculo.
const logoErrors = reactive<Record<string, boolean>>({})
const markLogoError = (vehicleId: string) => {
  logoErrors[vehicleId] = true
}
</script>

<template>
  <Card class="h-fit lg:sticky ">
    <CardHeader>
      <CardTitle>Veículos</CardTitle>
    </CardHeader>
    <CardContent class="flex max-h-[60vh] flex-col gap-4 overflow-y-auto">
      <div v-for="group in visibleGroups" :key="group.label">
        <button
          type="button"
          class="mb-2 flex w-full items-center justify-between text-xs font-semibold uppercase tracking-wide text-muted-foreground"
          @click="toggleGroup(group.label)"
        >
          <span>{{ group.label }} </span>
          <svg
            :class="['h-3 w-3 shrink-0 transition-transform', isOpen(group.label) ? 'rotate-180' : '']"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
          >
            <path d="m6 9 6 6 6-6" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>
        <div v-show="isOpen(group.label)" class="flex flex-col gap-2">
          <button
            v-for="vehicle in group.items"
            :key="vehicle.id"
            :class="[
              'flex items-center gap-3 rounded-md border-2 p-3 text-left transition-colors',
              vehicle.id === selectedId
                ? 'border-primary bg-primary/10'
                : 'border-transparent bg-muted hover:bg-accent',
            ]"
            @click="selectedId = vehicle.id"
          >
            <div class="flex h-9 w-9 shrink-0 items-center justify-center overflow-hidden rounded-md bg-secondary text-sm font-semibold">
              <img
                v-if="!logoErrors[vehicle.id]"
                :src="logoFor(vehicle)"
                :alt="vehicle.brand"
                class="h-full w-full object-contain p-1"
                @error="markLogoError(vehicle.id)"
              />
              <span v-else>{{ vehicle.brand.substring(0, 1) }}</span>
            </div>
            <div class="flex min-w-0 flex-col">
              <span class="truncate text-sm font-medium">{{ fullName(vehicle) }}</span>
              <span class="text-xs text-muted-foreground">{{ vehicle.plate }}</span>
            </div>
          </button>
        </div>
      </div>
    </CardContent>
    <div class="border-t border-border p-4">
      <Button class="w-full" variant="outline">+ Adicionar veículo</Button>
    </div>
  </Card>
</template>
