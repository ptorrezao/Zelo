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
  <div>
    <aside>
      <div>
        <div>Z</div>
      </div>
      <nav>
        <button
          v-for="item in navItems"
          :key="item.origin"
          :class="{ active: isActive(item.origin) }"
          @click="navigateTo(item)"
        >
          <span>{{ item.icon }}</span>
          <span>{{ item.label }}</span>
        </button>
      </nav>
      <div>
        <button @click="handleLogout">
          <span>↗</span>
          <span>Logout</span>
        </button>
      </div>
    </aside>

    <div>
      <nav>
        <ol>
          <li v-for="(item, index) in breadcrumbs" :key="index">
            <a v-if="!item.isLast" :href="item.href">{{ item.label }}</a>
            <span v-else>{{ item.label }}</span>
            <span v-if="index < breadcrumbs.length - 1" aria-hidden="true">/</span>
          </li>
        </ol>
      </nav>
      <div>
        <Transition name="page" mode="out-in">
          <slot :key="route.path" />
        </Transition>
      </div>
    </div>
  </div>
</template>
