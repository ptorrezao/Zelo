<script setup lang="ts">
import { cn } from '../../lib/utils'

interface Props {
  class?: string
  initials?: string
  src?: string
  alt?: string
}

defineProps<Props>()
</script>

<template>
  <div :class="cn('relative inline-flex h-10 w-10 shrink-0 overflow-hidden rounded-full bg-muted', $attrs.class)">
    <img v-if="src" :src="src" :alt="alt" class="aspect-square h-full w-full object-cover" />
    <span v-else class="flex h-full w-full items-center justify-center bg-muted text-xs font-medium">
      {{ initials }}
    </span>
  </div>
</template>
