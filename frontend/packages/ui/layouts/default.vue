<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useRuntimeConfig, useRequestURL, useCookie } from '#app'
import { Box, Home, LogOut, Truck } from '@lucide/vue'
import { ACCESS_TOKEN_COOKIE, REFRESH_TOKEN_COOKIE } from '../composables/useApiClient'
import SidebarProvider from '../components/ui/SidebarProvider.vue'
import Sidebar from '../components/ui/Sidebar.vue'
import SidebarRail from '../components/ui/SidebarRail.vue'
import SidebarInset from '../components/ui/SidebarInset.vue'
import SidebarHeader from '../components/ui/SidebarHeader.vue'
import SidebarContent from '../components/ui/SidebarContent.vue'
import SidebarFooter from '../components/ui/SidebarFooter.vue'
import SidebarGroup from '../components/ui/SidebarGroup.vue'
import SidebarGroupContent from '../components/ui/SidebarGroupContent.vue'
import SidebarMenu from '../components/ui/SidebarMenu.vue'
import SidebarMenuItem from '../components/ui/SidebarMenuItem.vue'
import SidebarMenuButton from '../components/ui/SidebarMenuButton.vue'
import SidebarTrigger from '../components/ui/SidebarTrigger.vue'

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
  { label: 'Início', path: '/', icon: Home, origin: zelo.shell },
  { label: 'Auto', path: '/', icon: Truck, origin: zelo.auto },
  { label: 'Inventário', path: '/', icon: Box, origin: zelo.inventory },
]

const isActive = (origin: string) => requestUrl.origin === origin

const handleLogout = () => {
  // So daqui - nao pela useAuth() da shell, porque este layout tambem
  // corre nas apps auto/inventario, que nao a tem. O cookie e o unico
  // estado de sessao que estas apps partilham entre si.
  useCookie(ACCESS_TOKEN_COOKIE).value = null
  useCookie(REFRESH_TOKEN_COOKIE).value = null
  window.location.href = zelo.shell + '/login'
}
</script>

<template>
  <SidebarProvider class="bg-muted/30">
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <div class="flex h-8 items-center gap-2 px-2 text-lg font-bold">
          <span class="flex h-6 w-6 shrink-0 items-center justify-center">Z</span>
          <span class="group-data-[collapsible=icon]:hidden">Zelo</span>
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarMenuItem v-for="item in navItems" :key="item.origin">
                <SidebarMenuButton
                  :href="item.origin + item.path"
                  :is-active="isActive(item.origin)"
                  :tooltip="item.label"
                >
                  <component :is="item.icon" class="h-4 w-4" />
                  <span>{{ item.label }}</span>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton tooltip="Logout" @click="handleLogout">
              <LogOut class="h-4 w-4" />
              <span>Logout</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>

    <SidebarInset>
      <nav class="flex h-12 items-center gap-3 border-b border-border bg-background px-4 text-sm">
        <SidebarTrigger />
        <ol class="flex items-center gap-2">
          <li v-for="(item, index) in breadcrumbs" :key="index" class="flex items-center gap-2">
            <a v-if="!item.isLast" :href="item.href" class="text-muted-foreground hover:text-foreground">{{ item.label }}</a>
            <span v-else class="font-medium text-foreground">{{ item.label }}</span>
            <span v-if="index < breadcrumbs.length - 1" class="text-muted-foreground" aria-hidden="true">/</span>
          </li>
        </ol>
      </nav>
      <div class="flex-1 overflow-y-auto p-6">
        <slot />
      </div>
    </SidebarInset>
  </SidebarProvider>
</template>
