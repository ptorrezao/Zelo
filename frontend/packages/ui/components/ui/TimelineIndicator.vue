<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  isLast?: boolean
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  isLast: false,
})
</script>

<template>
  <div class="relative flex justify-center">
    <span :class="cn('z-10 mt-1.5 h-2.5 w-2.5 shrink-0 rounded-full bg-muted-foreground', props.class)" />
    <span v-if="!isLast" class="absolute top-4 h-[calc(100%+0.75rem)] w-px bg-border" aria-hidden="true" />
  </div>
</template>
