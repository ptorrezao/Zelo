<script setup lang="ts">
import { cn } from '../../lib/utils'
import { useSidebar } from '../../composables/useSidebar'

interface Props {
  class?: string
}

const props = defineProps<Props>()

const { toggleSidebar } = useSidebar()
</script>

<template>
  <button
    type="button"
    aria-label="Alternar barra lateral"
    title="Alternar barra lateral"
    :class="cn(
      'absolute inset-y-0 z-20 hidden w-4 -translate-x-1/2 cursor-ew-resize transition-all ease-linear group-data-[side=left]:-right-4 group-data-[side=right]:left-0 after:absolute after:inset-y-0 after:left-1/2 after:w-[2px] hover:after:bg-sidebar-border sm:flex',
      'group-data-[collapsible=offcanvas]:translate-x-0 group-data-[collapsible=offcanvas]:after:left-full group-data-[collapsible=offcanvas]:hover:bg-sidebar',
      props.class,
    )"
    @click="toggleSidebar"
  />
</template>
