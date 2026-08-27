<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <ul :class="cn('flex w-full min-w-0 flex-col gap-1', props.class)">
    <slot />
  </ul>
</template>
