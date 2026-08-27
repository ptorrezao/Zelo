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
  <li :class="cn('relative grid grid-cols-[1.25rem_1fr] gap-x-4', isLast ? 'pb-0' : 'pb-6', props.class)">
    <slot />
  </li>
</template>
