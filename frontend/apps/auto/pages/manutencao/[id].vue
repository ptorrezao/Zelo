<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import { useVehicles } from '../../composables/useVehicles'

const route = useRoute()
const { findMaintenance, fullName } = useVehicles()

const result = computed(() => findMaintenance(route.params.id as string))
const vehicle = computed(() => result.value?.vehicle)
const maintenance = computed(() => result.value?.maintenance)

const typeColor: Record<string, string> = {
  preventiva: 'bg-primary',
  corretiva: 'bg-amber-500',
  inspecao: 'bg-emerald-500',
}

const typeLabel: Record<string, string> = {
  preventiva: 'Preventiva',
  corretiva: 'Corretiva',
  inspecao: 'Inspeção',
}
</script>

<template>
  <div v-if="maintenance && vehicle" class="mx-auto flex max-w-2xl flex-col gap-6">
    <div>
      <NuxtLink to="/" class="text-sm text-muted-foreground transition-colors hover:text-foreground">
        ← {{ fullName(vehicle) }}
      </NuxtLink>
    </div>

    <Card>
      <CardHeader class="flex flex-row items-start justify-between space-y-0">
        <div>
          <CardTitle>{{ maintenance.description }}</CardTitle>
          <p class="mt-1 text-sm text-muted-foreground">{{ maintenance.date }} · {{ fullName(vehicle) }} · {{ vehicle.plate }}</p>
        </div>
        <span :class="['shrink-0 rounded px-2 py-0.5 text-xs font-semibold text-white', typeColor[maintenance.type] || 'bg-muted-foreground']">
          {{ typeLabel[maintenance.type] || maintenance.type }}
        </span>
      </CardHeader>
      <CardContent class="flex flex-col gap-6">
        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Oficina</span>
            <span class="text-sm font-medium">{{ maintenance.workshop }}</span>
          </div>
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Quilómetros</span>
            <span class="text-sm font-medium">{{ maintenance.odometer }}</span>
          </div>
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Custo total</span>
            <span class="text-sm font-medium">{{ maintenance.cost }} €</span>
          </div>
          <div v-if="maintenance.invoice" class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Fatura</span>
            <span class="text-sm font-medium">{{ maintenance.invoice.number }}</span>
          </div>
        </div>

        <div v-if="maintenance.items?.length" class="border-t border-border pt-4">
          <h3 class="mb-3 text-sm font-semibold">Itens</h3>
          <div class="flex flex-col gap-2">
            <div
              v-for="(item, idx) in maintenance.items"
              :key="idx"
              class="flex items-center justify-between rounded-md bg-muted px-3 py-2"
            >
              <div class="flex flex-col">
                <span class="text-sm">{{ item.description }}</span>
                <span v-if="item.serialNumber" class="text-xs text-muted-foreground">N/S: {{ item.serialNumber }}</span>
              </div>
              <span class="text-sm font-medium">{{ item.price }} €</span>
            </div>
          </div>
        </div>

        <div v-if="maintenance.invoice" class="flex items-center justify-between rounded-md border border-border p-3">
          <div class="flex flex-col">
            <span class="text-sm font-medium">Fatura {{ maintenance.invoice.number }}</span>
            <span class="text-xs text-muted-foreground">{{ maintenance.invoice.date }}</span>
          </div>
          <a :href="maintenance.invoice.url" class="text-sm font-medium text-primary hover:underline">Ver fatura</a>
        </div>
      </CardContent>
    </Card>
  </div>

  <div v-else class="mx-auto max-w-2xl text-center text-sm text-muted-foreground">
    Manutenção não encontrada.
    <NuxtLink to="/" class="font-medium text-foreground hover:underline">Voltar</NuxtLink>
  </div>
</template>
