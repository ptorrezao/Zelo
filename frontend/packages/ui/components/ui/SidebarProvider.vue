<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, provide, ref } from 'vue'
import { useCookie } from '#app'
import { cn } from '../../lib/utils'
import {
  SIDEBAR_COOKIE_MAX_AGE,
  SIDEBAR_COOKIE_NAME,
  SIDEBAR_KEYBOARD_SHORTCUT,
  SIDEBAR_WIDTH,
  SIDEBAR_WIDTH_ICON,
  SidebarContextKey,
} from '../../composables/useSidebar'

interface Props {
  defaultOpen?: boolean
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  defaultOpen: true,
})

// Cookie em vez de localStorage: o servidor lê-o e já renderiza a sidebar
// no estado certo, sem o salto que a leitura no cliente provocaria.
const openCookie = useCookie<boolean>(SIDEBAR_COOKIE_NAME, {
  default: () => props.defaultOpen,
  maxAge: SIDEBAR_COOKIE_MAX_AGE,
  path: '/',
  sameSite: 'lax',
})

const open = ref(openCookie.value)
const setOpen = (value: boolean) => {
  open.value = value
  openCookie.value = value
}

const isMobile = ref(false)
const openMobile = ref(false)
const setOpenMobile = (value: boolean) => {
  openMobile.value = value
}

const toggleSidebar = () => {
  if (isMobile.value) {
    setOpenMobile(!openMobile.value)
  } else {
    setOpen(!open.value)
  }
}

const state = computed(() => (open.value ? 'expanded' : 'collapsed'))

provide(SidebarContextKey, {
  state,
  open,
  setOpen,
  isMobile,
  openMobile,
  setOpenMobile,
  toggleSidebar,
})

let mediaQuery: MediaQueryList | undefined
const updateIsMobile = () => {
  isMobile.value = mediaQuery?.matches ?? false
}
const handleKeydown = (event: KeyboardEvent) => {
  if (event.key === SIDEBAR_KEYBOARD_SHORTCUT && (event.metaKey || event.ctrlKey)) {
    event.preventDefault()
    toggleSidebar()
  }
}

onMounted(() => {
  mediaQuery = window.matchMedia('(max-width: 767px)')
  updateIsMobile()
  mediaQuery.addEventListener('change', updateIsMobile)
  window.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  mediaQuery?.removeEventListener('change', updateIsMobile)
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <div
    :style="{
      '--sidebar-width': SIDEBAR_WIDTH,
      '--sidebar-width-icon': SIDEBAR_WIDTH_ICON,
    }"
    :class="cn('flex min-h-svh w-full', props.class)"
  >
    <slot />
  </div>
</template>
