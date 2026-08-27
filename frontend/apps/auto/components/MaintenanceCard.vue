<script setup lang="ts">
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import Timeline from '@zelo/ui/components/ui/Timeline.vue'
import TimelineItem from '@zelo/ui/components/ui/TimelineItem.vue'
import TimelineIndicator from '@zelo/ui/components/ui/TimelineIndicator.vue'
import TimelineContent from '@zelo/ui/components/ui/TimelineContent.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected } = useVehicles()

const typeColor: Record<string, string> = {
  Preventiva: 'bg-primary',
  Correctiva: 'bg-amber-500',
  Inspeção: 'bg-emerald-500',
}
</script>

<template>
  <Card>
    <CardHeader class="flex flex-row items-center justify-between space-y-0">
      <CardTitle>Manutenção</CardTitle>
      <Button size="sm">+ Adicionar</Button>
    </CardHeader>
    <CardContent>
      <Timeline>
        <TimelineItem
          v-for="(entry, index) in selected?.maintenances"
          :key="entry.date"
          :is-last="index === selected!.maintenances.length - 1"
        >
          <TimelineIndicator :is-last="index === selected!.maintenances.length - 1" :class="typeColor[entry.type] || 'bg-muted-foreground'" />
          <TimelineContent>
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
          </TimelineContent>
        </TimelineItem>
      </Timeline>
    </CardContent>
  </Card>
</template>
