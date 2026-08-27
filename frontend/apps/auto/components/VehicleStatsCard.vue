<script setup lang="ts">
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, formatConsumption, formatKms, formatCost } = useVehicles()
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Estatísticas do carro</CardTitle>
    </CardHeader>
    <CardContent>
      <div class="mb-6 flex flex-col gap-2">
        <div class="flex items-center justify-between rounded-md bg-muted px-3 py-2">
          <span class="text-sm text-muted-foreground">Quilómetros (últimos 30 dias)</span>
          <span class="text-sm font-semibold">{{ formatKms(selected?.stats?.kmsLastMonth) }}</span>
        </div>
        <div class="flex items-center justify-between rounded-md bg-muted px-3 py-2">
          <span class="text-sm text-muted-foreground">Média/dia</span>
          <span class="text-sm font-semibold">{{ selected?.stats?.avgKmPerDay?.toFixed(1) }} km</span>
        </div>
        <div class="flex items-center justify-between rounded-md bg-muted px-3 py-2">
          <span class="text-sm text-muted-foreground">Consumo médio</span>
          <span class="text-sm font-semibold">{{ formatConsumption(selected?.stats?.avgConsumption) }}</span>
        </div>
        <div class="flex items-center justify-between rounded-md bg-muted px-3 py-2">
          <span class="text-sm text-muted-foreground">Custos manutenção (últimos 30 dias)</span>
          <span class="text-sm font-semibold">{{ formatCost(selected?.stats?.maintenanceCostLastMonth) }}</span>
        </div>
      </div>

      <div class="border-t border-border pt-4">
        <p class="mb-4 text-sm font-medium text-muted-foreground">Quilómetros por mês</p>
        <div class="flex h-[120px] items-end justify-around gap-1">
          <div
            v-for="(km, idx) in selected?.stats?.monthlyKms"
            :key="idx"
            class="min-h-1 flex-1 rounded-sm bg-primary"
            :style="{ height: (km / 150) + '%' }"
          ></div>
        </div>
      </div>
    </CardContent>
  </Card>
</template>
