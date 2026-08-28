<script setup lang="ts">
import VehicleListCard from '../components/VehicleListCard.vue'
import VehicleHeader from '../components/VehicleHeader.vue'
import VehicleDetailsCard from '../components/VehicleDetailsCard.vue'
import MaintenanceCard from '../components/MaintenanceCard.vue'
import VehicleStatsCard from '../components/VehicleStatsCard.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, isLoaded } = useVehicles()
</script>

<template>
  <div class="grid grid-cols-1 gap-6 lg:grid-cols-[300px_1fr]">
    <VehicleListCard />

    <main v-if="selected" class="flex flex-col gap-6">
      <VehicleHeader />

      <VehicleDetailsCard />

      <div class="grid grid-cols-1 items-stretch gap-6 lg:grid-cols-2">
        <MaintenanceCard />
        <VehicleStatsCard />
      </div>
    </main>

    <main v-else class="flex items-center justify-center rounded-lg border border-dashed border-border p-12 text-sm text-muted-foreground">
      {{ isLoaded ? 'Sem veículos ainda — adicione o primeiro.' : 'A carregar...' }}
    </main>
  </div>
</template>
