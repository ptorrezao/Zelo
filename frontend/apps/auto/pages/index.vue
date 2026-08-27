<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import VehicleListCard from '../components/VehicleListCard.vue'
import VehicleDetailsCard from '../components/VehicleDetailsCard.vue'

const {
  selected,
  fullName,
  formatConsumption,
  formatKms,
  formatCost,
} = useVehicles()

const typeColor: Record<string, string> = {
  Preventiva: 'bg-primary',
  Correctiva: 'bg-amber-500',
  Inspeção: 'bg-emerald-500',
}
</script>

<template>
  <div class="grid grid-cols-1 gap-6 lg:grid-cols-[300px_1fr]">
    <VehicleListCard />

    <main class="flex flex-col gap-6">
      <div class="flex items-center gap-4 rounded-lg border border-border bg-card p-6 shadow-sm">
        <Avatar :name="selected?.driver || 'PT'" class="h-12 w-12 text-base" />
        <div>
          <h1 class="text-xl font-semibold">{{ fullName(selected) }}</h1>
          <p class="text-sm text-muted-foreground">ID: {{ selected?.id }}</p>
        </div>
      </div>

      <div class="grid grid-cols-1 items-stretch gap-6 lg:grid-cols-2">
        <VehicleDetailsCard />

        <div class="flex flex-col gap-6">
          <Card>
            <CardHeader class="flex flex-row items-center justify-between space-y-0">
              <CardTitle>Manutenção</CardTitle>
              <Button size="sm">+ Adicionar</Button>
            </CardHeader>
            <CardContent class="flex flex-col gap-4">
              <div v-for="entry in selected?.maintenances" :key="entry.date" class="flex gap-4">
                <div class="pt-1">
                  <div :class="['h-3 w-3 rounded-full', typeColor[entry.type] || 'bg-muted-foreground']"></div>
                </div>
                <div class="flex-1">
                  <div class="mb-1 flex items-center justify-between">
                    <span class="text-sm font-semibold">{{ entry.date }}</span>
                    <span
                      :class="[
                        'rounded px-2 py-0.5 text-xs font-semibold capitalize text-white',
                        typeColor[entry.type] || 'bg-muted-foreground',
                      ]"
                    >
                      {{ entry.type }}
                    </span>
                  </div>
                  <p class="mb-1 text-sm font-medium">{{ entry.description }}</p>
                  <div class="flex flex-col gap-0.5 text-xs text-muted-foreground">
                    <span>Oficina: {{ entry.workshop }}</span>
                    <span>Quilómetros: {{ entry.odometer }}</span>
                    <span>Custo: {{ entry.cost }} €</span>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

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
        </div>
      </div>
    </main>
  </div>
</template>
