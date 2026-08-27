<script setup lang="ts">
import { computed, ref } from 'vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import Timeline from '@zelo/ui/components/ui/Timeline.vue'
import TimelineItem from '@zelo/ui/components/ui/TimelineItem.vue'
import TimelineIndicator from '@zelo/ui/components/ui/TimelineIndicator.vue'
import TimelineContent from '@zelo/ui/components/ui/TimelineContent.vue'
import AddMaintenanceSheet from './AddMaintenanceSheet.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected } = useVehicles()

const isAddOpen = ref(false)

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

const VISIBLE_LIMIT = 3
const expanded = ref(false)

const maintenances = computed(() => selected.value?.maintenances ?? [])
const hasMore = computed(() => maintenances.value.length > VISIBLE_LIMIT)
const visibleMaintenances = computed(() =>
  expanded.value ? maintenances.value : maintenances.value.slice(0, VISIBLE_LIMIT),
)
</script>

<template>
  <Card>
    <CardHeader class="flex flex-row items-center justify-between space-y-0">
      <CardTitle>Manutenção</CardTitle>
      <Button size="sm" @click="isAddOpen = true">+ Adicionar</Button>
    </CardHeader>
    <CardContent>
      <Timeline>
        <TimelineItem
          v-for="(entry, index) in visibleMaintenances"
          :key="entry.id"
          :is-last="index === visibleMaintenances.length - 1"
        >
          <TimelineIndicator
            :is-last="index === visibleMaintenances.length - 1"
            :class="typeColor[entry.type] || 'bg-muted-foreground'"
          />
          <TimelineContent>
            <NuxtLink :to="`/manutencao/${entry.id}`" class="-m-2 block rounded-md p-2 transition-colors hover:bg-accent">
              <div class="mb-1 flex items-center justify-between">
                <span class="text-sm font-semibold">{{ entry.date }}</span>
                <span
                  :class="[
                    'rounded px-2 py-0.5 text-xs font-semibold text-white',
                    typeColor[entry.type] || 'bg-muted-foreground',
                  ]"
                >
                  {{ typeLabel[entry.type] || entry.type }}
                </span>
              </div>
              <p class="mb-1 text-sm font-medium">{{ entry.description }}</p>
              <div class="flex flex-col gap-0.5 text-xs text-muted-foreground">
                <span>Oficina: {{ entry.workshop }}</span>
                <span>Quilómetros: {{ entry.odometer }}</span>
                <span>Custo: {{ entry.cost }} €</span>
              </div>
            </NuxtLink>
          </TimelineContent>
        </TimelineItem>
      </Timeline>

      <Button
        v-if="hasMore"
        variant="ghost"
        size="sm"
        class="mt-4 w-full text-muted-foreground"
        @click="expanded = !expanded"
      >
        {{ expanded ? 'Ver menos' : `Ver mais (${maintenances.length - VISIBLE_LIMIT})` }}
      </Button>
    </CardContent>
  </Card>

  <AddMaintenanceSheet v-model:open="isAddOpen" />
</template>
