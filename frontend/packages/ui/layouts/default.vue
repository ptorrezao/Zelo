<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useRuntimeConfig, useRequestURL } from '#app'

const route = useRoute()
const config = useRuntimeConfig()
const zelo = config.public.zelo as { shell: string; auto: string; inventory: string }
const requestUrl = useRequestURL()

const breadcrumbs = computed(() => {
  const parts = route.path.split('/').filter(Boolean)
  if (parts.length === 0) {
    return [{ label: 'Home', href: '/', isLast: true }]
  }
  const appName = parts[0]
  return [
    { label: 'Home', href: '/' },
    { label: appName.charAt(0).toUpperCase() + appName.slice(1), href: `/${appName}`, isLast: true },
  ]
})

const navItems = [
  { label: 'Início', path: '/', icon: '⌂', origin: zelo.shell },
  { label: 'Auto', path: '/', icon: '◆', origin: zelo.auto },
  { label: 'Inventário', path: '/', icon: '≡', origin: zelo.inventory },
]

const isActive = (origin: string) => requestUrl.origin === origin

const navigateTo = (item: typeof navItems[number]) => {
  window.location.href = item.origin + item.path
}

const handleLogout = () => {
  window.location.href = zelo.shell + '/login'
}
</script>

<template>
  <div class="flex min-h-screen bg-muted/30">
    <aside class="fixed inset-y-0 left-0 z-20 flex w-16 flex-col bg-sidebar text-sidebar-foreground">
      <div class="flex h-16 items-center justify-center text-lg font-bold">Z</div>
      <nav class="flex flex-1 flex-col gap-1 px-2">
        <button
          v-for="item in navItems"
          :key="item.origin"
          :class="[
            'flex h-10 items-center justify-center rounded-md text-sidebar-foreground/70 transition-colors hover:bg-white/10 hover:text-sidebar-foreground',
            isActive(item.origin) && 'bg-primary text-primary-foreground hover:bg-primary/90',
          ]"
          :title="item.label"
          @click="navigateTo(item)"
        >
          <span class="text-lg">{{ item.icon }}</span>
        </button>
      </nav>
      <div class="flex justify-center p-2">
        <button
          class="flex h-10 w-10 items-center justify-center rounded-md text-sidebar-foreground/70 transition-colors hover:bg-white/10 hover:text-sidebar-foreground"
          title="Logout"
          @click="handleLogout"
        >
          <span class="text-lg">↗</span>
        </button>
      </div>
    </aside>

    <div class="flex flex-1 flex-col pl-16">
      <nav class="flex h-12 items-center border-b border-border bg-background px-6 text-sm">
        <ol class="flex items-center gap-2">
          <li v-for="(item, index) in breadcrumbs" :key="index" class="flex items-center gap-2">
            <a v-if="!item.isLast" :href="item.href" class="text-muted-foreground hover:text-foreground">{{ item.label }}</a>
            <span v-else class="font-medium text-foreground">{{ item.label }}</span>
            <span v-if="index < breadcrumbs.length - 1" class="text-muted-foreground" aria-hidden="true">/</span>
          </li>
        </ol>
      </nav>
      <div class="flex-1 overflow-y-auto p-6">
        <Transition name="page" mode="out-in">
          <slot :key="route.path" />
        </Transition>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-enter-active,
.page-leave-active {
  transition: opacity 0.2s ease;
}
.page-enter-from,
.page-leave-to {
  opacity: 0;
}
</style>
