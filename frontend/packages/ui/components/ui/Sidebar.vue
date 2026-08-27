<script setup lang="ts">
import { cn } from '../../lib/utils'
import { SIDEBAR_WIDTH_MOBILE, useSidebar } from '../../composables/useSidebar'

interface Props {
  side?: 'left' | 'right'
  variant?: 'sidebar' | 'floating' | 'inset'
  collapsible?: 'offcanvas' | 'icon' | 'none'
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  side: 'left',
  variant: 'sidebar',
  collapsible: 'offcanvas',
})

const { isMobile, state, openMobile, setOpenMobile } = useSidebar()
</script>

<template>
  <div
    v-if="collapsible === 'none'"
    :class="cn('flex h-full w-[var(--sidebar-width)] flex-col bg-sidebar text-sidebar-foreground', props.class)"
  >
    <slot />
  </div>

  <Teleport v-else-if="isMobile" to="body">
    <Transition name="sidebar-backdrop">
      <div
        v-if="openMobile"
        class="fixed inset-0 z-40 bg-black/50"
        @click="setOpenMobile(false)"
      />
    </Transition>
    <Transition :name="side === 'left' ? 'sidebar-slide-left' : 'sidebar-slide-right'">
      <div
        v-if="openMobile"
        :style="{ width: SIDEBAR_WIDTH_MOBILE }"
        :class="cn(
          'fixed inset-y-0 z-50 flex flex-col bg-sidebar text-sidebar-foreground shadow-xl',
          side === 'left' ? 'left-0' : 'right-0',
        )"
      >
        <slot />
      </div>
    </Transition>
  </Teleport>

  <div
    v-else
    class="group peer hidden text-sidebar-foreground md:block"
    :data-state="state"
    :data-collapsible="state === 'collapsed' ? collapsible : ''"
    :data-variant="variant"
    :data-side="side"
  >
    <!-- Spacer invisível: reserva o espaço que a sidebar fixed ocupa, para
         o conteúdo ao lado deslocar-se com ela em vez de ficar por baixo. -->
    <div
      :class="cn(
        'relative h-svh w-[var(--sidebar-width)] bg-transparent transition-[width] duration-200 ease-linear',
        'group-data-[collapsible=offcanvas]:w-0',
        'group-data-[side=right]:rotate-180',
        variant === 'floating' || variant === 'inset'
          ? 'group-data-[collapsible=icon]:w-[calc(var(--sidebar-width-icon)+1rem)]'
          : 'group-data-[collapsible=icon]:w-[var(--sidebar-width-icon)]',
      )"
    />
    <div
      :class="cn(
        'fixed inset-y-0 z-10 hidden h-svh w-[var(--sidebar-width)] transition-[left,right,width] duration-200 ease-linear md:flex',
        side === 'left'
          ? 'left-0 group-data-[collapsible=offcanvas]:left-[calc(var(--sidebar-width)*-1)]'
          : 'right-0 group-data-[collapsible=offcanvas]:right-[calc(var(--sidebar-width)*-1)]',
        variant === 'floating' || variant === 'inset'
          ? 'p-2 group-data-[collapsible=icon]:w-[calc(var(--sidebar-width-icon)+1rem+2px)]'
          : 'group-data-[collapsible=icon]:w-[var(--sidebar-width-icon)] group-data-[side=left]:border-r group-data-[side=right]:border-l border-sidebar-border',
        props.class,
      )"
    >
      <div
        :class="cn(
          'flex h-full w-full flex-col bg-sidebar',
          variant === 'floating' && 'rounded-lg border border-sidebar-border shadow-sm',
        )"
      >
        <slot />
      </div>
    </div>
  </div>
</template>

<style scoped>
.sidebar-backdrop-enter-active,
.sidebar-backdrop-leave-active {
  transition: opacity 200ms ease;
}
.sidebar-backdrop-enter-from,
.sidebar-backdrop-leave-to {
  opacity: 0;
}

.sidebar-slide-left-enter-active,
.sidebar-slide-left-leave-active,
.sidebar-slide-right-enter-active,
.sidebar-slide-right-leave-active {
  transition: transform 200ms ease;
}
.sidebar-slide-left-enter-from,
.sidebar-slide-left-leave-to {
  transform: translateX(-100%);
}
.sidebar-slide-right-enter-from,
.sidebar-slide-right-leave-to {
  transform: translateX(100%);
}
</style>
