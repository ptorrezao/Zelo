<script setup lang="ts">
import { computed } from 'vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardDescription from '@zelo/ui/components/ui/CardDescription.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import { useVehicles } from '../composables/useVehicles'

const { selected, fullName } = useVehicles()

const documentsByCategory = computed(() => {
  const groups = new Map<string, typeof selected.value.documents>()
  for (const doc of selected.value?.documents ?? []) {
    if (!groups.has(doc.category)) groups.set(doc.category, [])
    groups.get(doc.category)!.push(doc)
  }
  return Array.from(groups.entries()).map(([category, documents]) => ({ category, documents }))
})

const typeIcon: Record<string, string> = {
  pdf: '📄',
  imagem: '🖼️',
}
</script>

<template>
  <div class="mx-auto flex max-w-3xl flex-col gap-6">
    <div>
      <NuxtLink to="/" class="text-sm text-muted-foreground transition-colors hover:text-foreground">
        ← {{ fullName(selected) }}
      </NuxtLink>
    </div>

    <Card>
      <CardHeader>
        <CardTitle>Documentos</CardTitle>
        <CardDescription>{{ fullName(selected) }} · {{ selected?.plate }}</CardDescription>
      </CardHeader>
      <CardContent class="flex flex-col gap-6">
        <p v-if="!selected?.documents?.length" class="text-sm text-muted-foreground">
          Nenhum documento disponível para este veículo.
        </p>

        <div v-for="group in documentsByCategory" :key="group.category" class="flex flex-col gap-2">
          <h3 class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">{{ group.category }}</h3>
          <div class="flex flex-col divide-y divide-border rounded-md border border-border">
            <a
              v-for="doc in group.documents"
              :key="doc.id"
              href="#"
              class="flex items-center gap-3 p-3 transition-colors hover:bg-accent"
            >
              <span class="text-xl">{{ typeIcon[doc.type] || '📄' }}</span>
              <div class="flex min-w-0 flex-1 flex-col">
                <span class="truncate text-sm font-medium">{{ doc.name }}</span>
                <span class="text-xs text-muted-foreground">{{ doc.date }} · {{ doc.size }}</span>
              </div>
            </a>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
