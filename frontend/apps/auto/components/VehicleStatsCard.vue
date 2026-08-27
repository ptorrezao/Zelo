<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import ChartContainer from '@zelo/ui/components/ui/ChartContainer.vue'
import ChartTooltip from '@zelo/ui/components/ui/ChartTooltip.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, formatConsumption, formatKms, formatCost } = useVehicles()

const monthlyKms = computed(() => selected.value?.stats?.monthlyKms ?? [])
const maxKm = computed(() => Math.max(...monthlyKms.value.map(m => m.value), 1))

const chartConfig = {
  value: { label: 'Quilómetros', color: 'hsl(var(--primary))' },
}

const hovered = ref<number | null>(null)
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
        <ChartContainer :config="chartConfig" class="h-[140px]">
          <div class="flex h-full items-end justify-between gap-1.5" @mouseleave="hovered = null">
            <div
              v-for="(month, idx) in monthlyKms"
              :key="month.label"
              class="relative flex h-full flex-1 flex-col items-center justify-end gap-1.5"
              @mouseenter="hovered = idx"
            >
              <ChartTooltip v-if="hovered === idx" class="absolute bottom-full mb-2 whitespace-nowrap">
                <p class="font-medium text-foreground">{{ month.label }}</p>
                <p class="text-muted-foreground">{{ month.value.toLocaleString('pt-PT') }} km</p>
              </ChartTooltip>
              <div
                :class="[
                  'w-full rounded-sm bg-[var(--color-value)] transition-opacity',
                  hovered !== null && hovered !== idx ? 'opacity-40' : 'opacity-100',
                ]"
                :style="{ height: `${(month.value / maxKm) * 100}%` }"
              ></div>
              <span class="text-[10px] text-muted-foreground">{{ month.label.split(' ')[0] }}</span>
            </div>
          </div>
        </ChartContainer>
      </div>
    </CardContent>
  </Card>
</template>
