<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <div :class="cn('mt-auto flex flex-col-reverse gap-2 pt-4 sm:flex-row sm:justify-end', props.class)">
    <slot />
  </div>
</template>
