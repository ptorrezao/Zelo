<script setup lang="ts">
import VehicleListCard from '../components/VehicleListCard.vue'
import VehicleHeader from '../components/VehicleHeader.vue'
import VehicleDetailsCard from '../components/VehicleDetailsCard.vue'
import MaintenanceCard from '../components/MaintenanceCard.vue'
import VehicleStatsCard from '../components/VehicleStatsCard.vue'
</script>

<template>
  <div class="grid grid-cols-1 gap-6 lg:grid-cols-[300px_1fr]">
    <VehicleListCard />

    <main class="flex flex-col gap-6">
      <VehicleHeader />

      <VehicleDetailsCard />

      <div class="grid grid-cols-1 items-stretch gap-6 lg:grid-cols-2">
        <MaintenanceCard />
        <VehicleStatsCard />
      </div>
    </main>
  </div>
</template>
