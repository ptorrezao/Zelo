<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <h2 :class="cn('text-lg font-semibold', props.class)">
    <slot />
  </h2>
</template>
