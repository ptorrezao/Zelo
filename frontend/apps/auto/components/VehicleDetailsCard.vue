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
    <!-- Em mobile nao ha espaco para dividir a par do texto: a foto fica
         como fundo decorativo atras de tudo (object-right para nao cortar
         o carro a meio), com um veu solido para manter a leitura. -->
    <img
      v-if="photo"
      :src="photo"
      :alt="fullName(selected)"
      class="pointer-events-none absolute inset-0 h-full w-full object-cover object-right opacity-60 sm:hidden"
    />
    <div class="pointer-events-none absolute inset-0 bg-card/75 sm:hidden"></div>

    <!-- A partir do sm, colunas a serio lado a lado (nao fundo decorativo
         por baixo do texto) - a foto fica na sua propria caixa, com
         object-contain (nunca corta) alinhada a direita. -->
    <div class="relative flex flex-col sm:flex-row">
      <div class="min-w-0 flex-1">
        <CardHeader>
          <CardTitle>{{ fullName(selected) }}</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div class="flex flex-col gap-1">
              <span class="text-xs font-semibold uppercase text-muted-foreground">VIN</span>
              <span class="text-sm font-medium">{{ selected?.vin }}</span>
            </div>
            <div class="flex flex-col gap-1">
              <span class="text-xs font-semibold uppercase text-muted-foreground">Cor</span>
              <span class="text-sm font-medium">{{ selected?.color }}</span>
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
              <span class="text-xs font-semibold uppercase text-muted-foreground">Nº da apólice</span>
              <span class="text-sm font-medium">{{ selected?.insurancePolicyNumber }}</span>
            </div>
            <div class="flex flex-col gap-1">
              <span class="text-xs font-semibold uppercase text-muted-foreground">Período do seguro</span>
              <span class="text-sm font-medium">{{ selected?.insurancePeriodStart }} – {{ selected?.insurancePeriodEnd }}</span>
            </div>
            <div class="flex flex-col gap-1">
              <span class="text-xs font-semibold uppercase text-muted-foreground">Valor do prémio</span>
              <span class="text-sm font-medium">{{ selected?.insurancePremium }}</span>
            </div>
            <div class="flex flex-col gap-1">
              <span class="text-xs font-semibold uppercase text-muted-foreground">Próxima inspeção</span>
              <span class="text-sm font-medium">{{ selected?.nextInspection }}</span>
            </div>
            <div class="flex flex-col gap-1">
              <span class="text-xs font-semibold uppercase text-muted-foreground">Data do IUC</span>
              <span class="text-sm font-medium">{{ selected?.iucDueDate }}</span>
            </div>
          </div>

          <div class="flex items-center gap-4 border-y border-border py-4">
            <LicensePlate v-if="selected?.plate" :value="selected.plate" class="shrink-0" />
            <NuxtLink to="/documentos" class="text-sm font-medium text-primary hover:underline">Documentos</NuxtLink>
          </div>
        </CardContent>
      </div>

      <div v-if="photo" class="relative hidden shrink-0 sm:flex sm:max-w-[50%]">
        <!-- h-full w-auto (nao w-full) - a foto ocupa a altura toda do
             card e a largura segue o seu aspect ratio, sem recortar
             lateralmente. sm:max-w-[50%] e so uma rede de seguranca para
             fotos muito largas/baixas nao empurrarem o texto. -->
        <img :src="photo" :alt="fullName(selected)" class="h-full w-auto object-contain object-right" />
        <!-- Gradiente como no design original: a foto emerge do fundo do
             card em vez de comecar de repente numa borda dura. -->
        <div class="pointer-events-none absolute inset-0 bg-gradient-to-r from-card from-0% via-transparent via-45% to-transparent"></div>
      </div>
    </div>
  </Card>
</template>
