<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '../../lib/utils'

interface ChartConfigEntry {
  label: string
  color?: string
}

interface Props {
  config: Record<string, ChartConfigEntry>
  class?: string
}

const props = defineProps<Props>()

// Cada serie fica disponivel como --color-<chave> dentro do container,
// para as barras/linhas do grafico lerem via bg-[var(--color-x)].
const styleVars = computed(() => {
  const vars: Record<string, string> = {}
  for (const [key, value] of Object.entries(props.config)) {
    if (value.color) {
      vars[`--color-${key}`] = value.color
    }
  }
  return vars
})
</script>

<template>
  <div :class="cn('w-full text-xs', props.class)" :style="styleVars">
    <slot />
  </div>
</template>
