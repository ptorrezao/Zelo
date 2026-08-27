<script setup lang="ts">
import { PanelLeft } from '@lucide/vue'
import Button from './Button.vue'
import { cn } from '../../lib/utils'
import { useSidebar } from '../../composables/useSidebar'

interface Props {
  class?: string
}

const props = defineProps<Props>()

const { toggleSidebar } = useSidebar()
</script>

<template>
  <Button
    variant="ghost"
    size="icon"
    :class="cn('h-7 w-7', props.class)"
    aria-label="Alternar barra lateral"
    @click="toggleSidebar"
  >
    <PanelLeft class="h-4 w-4" />
  </Button>
</template>
