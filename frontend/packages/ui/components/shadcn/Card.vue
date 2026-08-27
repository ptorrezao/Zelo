<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
}

defineProps<Props>()
</script>

<template>
  <div :class="cn('rounded-md border bg-card text-card-foreground shadow-sm', $attrs.class)">
    <slot />
  </div>
</template>
