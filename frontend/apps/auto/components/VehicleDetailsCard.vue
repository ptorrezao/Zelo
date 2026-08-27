<script setup lang="ts">
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, photo, fullName } = useVehicles()
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>{{ fullName(selected) }}</CardTitle>
    </CardHeader>
    <CardContent>
      <div class="mb-6 grid grid-cols-2 gap-4">
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

      <div class="my-4 flex items-center gap-4 border-y border-border py-4">
        <div class="rounded-sm bg-blue-900 px-3 py-2 font-mono text-sm font-semibold text-white">
          {{ selected?.plate }}
        </div>
        <a href="#documentos" class="text-sm font-medium text-primary hover:underline">Documentos</a>
      </div>

      <div class="flex justify-center pt-4">
        <img v-if="photo" :src="photo" :alt="fullName(selected)" class="max-h-[300px] max-w-full rounded-md" />
      </div>
    </CardContent>
  </Card>
</template>
