<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
}

const props = defineProps<Props>()
</script>

<template>
  <ol :class="cn('flex flex-col', props.class)">
    <slot />
  </ol>
</template>
