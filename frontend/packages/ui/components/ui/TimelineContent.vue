<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <div :class="cn('flex flex-col gap-1 pt-0.5', props.class)">
    <slot />
  </div>
</template>
