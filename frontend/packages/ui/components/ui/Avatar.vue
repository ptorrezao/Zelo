<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '../../lib/utils'

interface Props {
  name?: string
  class?: string
}

const props = defineProps<Props>()

const initials = computed(() =>
  (props.name || '?')
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join(''),
)
</script>

<template>
  <div
    :class="cn(
      'flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-medium text-primary-foreground',
      props.class,
    )"
  >
    {{ initials }}
  </div>
</template>
