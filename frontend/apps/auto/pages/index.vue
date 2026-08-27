<script setup lang="ts">
import { useVehicles } from '../composables/useVehicles'
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import VehicleListCard from '../components/VehicleListCard.vue'
import VehicleDetailsCard from '../components/VehicleDetailsCard.vue'
import MaintenanceCard from '../components/MaintenanceCard.vue'
import VehicleStatsCard from '../components/VehicleStatsCard.vue'

const { selected, fullName } = useVehicles()
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

      <VehicleDetailsCard />

      <div class="grid grid-cols-1 items-stretch gap-6 lg:grid-cols-2">
        <MaintenanceCard />
        <VehicleStatsCard />
      </div>
    </main>
  </div>
</template>
