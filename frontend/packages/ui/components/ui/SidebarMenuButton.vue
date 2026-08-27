<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '../../lib/utils'

interface Props {
  href?: string
  isActive?: boolean
  variant?: 'default' | 'outline'
  size?: 'default' | 'sm' | 'lg'
  tooltip?: string
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  isActive: false,
  variant: 'default',
  size: 'default',
})

const variantClasses: Record<string, string> = {
  default: 'hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
  outline:
    'bg-background shadow-[0_0_0_1px_hsl(var(--sidebar-border))] hover:bg-sidebar-accent hover:text-sidebar-accent-foreground hover:shadow-[0_0_0_1px_hsl(var(--sidebar-accent))]',
}

const sizeClasses: Record<string, string> = {
  default: 'h-8 text-sm',
  sm: 'h-7 text-xs',
  lg: 'h-12 text-sm group-data-[collapsible=icon]:!p-0',
}

const classes = computed(() =>
  cn(
    'peer/menu-button flex w-full items-center gap-2 overflow-hidden rounded-md p-2 text-left outline-none ring-sidebar-ring transition-[width,height,padding] focus-visible:ring-2 disabled:pointer-events-none disabled:opacity-50 group-has-[[data-sidebar=menu-action]]/menu-item:pr-8 aria-disabled:pointer-events-none aria-disabled:opacity-50 group-data-[collapsible=icon]:!size-8 group-data-[collapsible=icon]:!p-2 [&>svg]:size-4 [&>svg]:shrink-0',
    variantClasses[props.variant],
    sizeClasses[props.size],
    props.isActive && 'bg-sidebar-accent font-medium text-sidebar-accent-foreground',
    props.class,
  ),
)
</script>

<template>
  <a
    v-if="href"
    :href="href"
    :data-active="isActive"
    :class="classes"
  >
    <slot />
    <span
      v-if="tooltip"
      class="pointer-events-none absolute left-full top-1/2 z-50 ml-2 hidden -translate-y-1/2 whitespace-nowrap rounded-md bg-popover px-2 py-1 text-xs text-popover-foreground opacity-0 shadow-md transition-opacity group-data-[collapsible=icon]:group-hover/menu-item:block group-data-[collapsible=icon]:group-hover/menu-item:opacity-100"
    >
      {{ tooltip }}
    </span>
  </a>
  <button
    v-else
    type="button"
    :data-active="isActive"
    :class="classes"
  >
    <slot />
    <span
      v-if="tooltip"
      class="pointer-events-none absolute left-full top-1/2 z-50 ml-2 hidden -translate-y-1/2 whitespace-nowrap rounded-md bg-popover px-2 py-1 text-xs text-popover-foreground opacity-0 shadow-md transition-opacity group-data-[collapsible=icon]:group-hover/menu-item:block group-data-[collapsible=icon]:group-hover/menu-item:opacity-100"
    >
      {{ tooltip }}
    </span>
  </button>
</template>
