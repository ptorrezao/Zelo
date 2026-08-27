<script setup lang="ts">
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import LicensePlate from './LicensePlate.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, photo, fullName } = useVehicles()
</script>

<template>
  <Card class="relative overflow-hidden">
    <!-- Foto do veiculo como fundo decorativo: ocupa sempre a altura toda
         do card, cortando na largura se for preciso, e esbate para a cor
         do card para nao tapar o texto. -->
    <img
      v-if="photo"
      :src="photo"
      :alt="fullName(selected)"
      class="pointer-events-none absolute inset-y-0 right-0 w-1/2 object-cover object-right opacity-40"
    />
    <div class="pointer-events-none absolute inset-0 bg-gradient-to-r from-card via-card/80 to-transparent"></div>

    <CardHeader class="relative">
      <CardTitle>{{ fullName(selected) }}</CardTitle>
    </CardHeader>
    <CardContent class="relative max-w-[65%] sm:max-w-[60%]">
      <div class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div class="flex flex-col gap-1">
          <span class="text-xs font-semibold uppercase text-muted-foreground">VIN</span>
          <span class="text-sm font-medium">{{ selected?.vin }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs font-semibold uppercase text-muted-foreground">Data de matrícula</span>
          <span class="text-sm font-medium">{{ selected?.registered }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs font-semibold uppercase text-muted-foreground">Quilómetros</span>
          <span class="text-sm font-medium">{{ selected?.odometer }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs font-semibold uppercase text-muted-foreground">Seguradora</span>
          <span class="text-sm font-medium">{{ selected?.insurer }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs font-semibold uppercase text-muted-foreground">Renovação do seguro</span>
          <span class="text-sm font-medium">{{ selected?.insuranceRenewal }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs font-semibold uppercase text-muted-foreground">Próxima inspeção</span>
          <span class="text-sm font-medium">{{ selected?.nextInspection }}</span>
        </div>
      </div>

      <div class="flex items-center gap-4 border-y border-border py-4">
        <LicensePlate v-if="selected?.plate" :value="selected.plate" />
        <a href="#documentos" class="text-sm font-medium text-primary hover:underline">Documentos</a>
      </div>
    </CardContent>
  </Card>
</template>
