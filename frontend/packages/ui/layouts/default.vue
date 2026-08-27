<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const breadcrumbs = computed(() => {
  const parts = route.path.split('/').filter(Boolean)

  if (parts.length === 0) {
    return [{ label: 'Home', href: '/', isLast: true }]
  }

  const appName = parts[0]
  return [
    { label: 'Home', href: '/' },
    {
      label: appName.charAt(0).toUpperCase() + appName.slice(1),
      href: `/${appName}`,
      isLast: true,
    },
  ]
})

const navItems = [
  { label: 'Início', path: '/', icon: '⌂' },
  { label: 'Auto', path: '/auto', icon: '◆' },
  { label: 'Inventário', path: '/inventory', icon: '≡' },
]

const isActive = (path: string) => route.path.startsWith(path)

const handleLogout = () => {
  router.push('/login')
}
</script>

<template>
  <div class="layout">
    <!-- Sidebar Navigation -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="sidebar-logo">Z</div>
      </div>

      <nav class="sidebar-nav">
        <button
          v-for="item in navItems"
          :key="item.path"
          :class="['nav-button', { active: isActive(item.path) }]"
          @click="router.push(item.path)"
          :title="item.label"
        >
          <span class="nav-icon">{{ item.icon }}</span>
          <span class="nav-text">{{ item.label }}</span>
        </button>
      </nav>

      <div class="sidebar-footer">
        <button class="logout-button" @click="handleLogout" title="Logout">
          <span class="logout-icon">↗</span>
          <span class="logout-text">Logout</span>
        </button>
      </div>
    </aside>

    <!-- Main Content -->
    <div class="main-wrapper">
      <nav class="breadcrumb" aria-label="Breadcrumb">
        <ol class="breadcrumb-list">
          <li v-for="(item, index) in breadcrumbs" :key="index" class="breadcrumb-item">
            <a v-if="!item.isLast" :href="item.href" class="breadcrumb-link">
              {{ item.label }}
            </a>
            <span v-else class="breadcrumb-text">
              {{ item.label }}
            </span>
            <span v-if="index < breadcrumbs.length - 1" class="breadcrumb-separator" aria-hidden="true">/</span>
          </li>
        </ol>
      </nav>

      <div class="content">
        <Transition name="page" mode="out-in">
          <slot :key="route.path" />
        </Transition>
      </div>
    </div>
  </div>
</template>

<style scoped>
.layout {
  display: flex;
  min-height: 100vh;
  background: #f5f5f5;
}

/* Sidebar */
.sidebar {
  position: fixed;
  left: 0;
  top: 0;
  width: 64px;
  height: 100vh;
  background: #1a1a1a;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  z-index: 1000;
  overflow-y: auto;
  overflow-x: hidden;
  transition: width 0.3s ease;
}

.sidebar:hover {
  width: 240px;
}

.sidebar-header {
  padding: 1.5rem 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 64px;
}

.sidebar:hover .sidebar-header {
  justify-content: flex-start;
}

.sidebar-logo {
  font-size: 1.5rem;
  font-weight: 700;
  color: #0066cc;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
}

.sidebar:hover .sidebar-logo {
  font-size: 1.25rem;
  width: auto;
  padding-right: 0.5rem;
}

.sidebar-nav {
  flex: 1;
  padding: 1rem 0;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 1rem 0.5rem;
}

.nav-button {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: rgba(255, 255, 255, 0.6);
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 40px;
  width: 40px;
  justify-content: center;
  font-family: inherit;
}

.sidebar:hover .nav-button {
  width: 100%;
  justify-content: flex-start;
  padding: 0.75rem 1rem;
}

.nav-button:hover {
  background: rgba(255, 255, 255, 0.08);
  color: rgba(255, 255, 255, 0.9);
}

.nav-button.active {
  background: #0066cc;
  color: white;
}

.nav-icon {
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.nav-text {
  display: none;
  font-size: 0.9rem;
  white-space: nowrap;
}

.sidebar:hover .nav-text {
  display: block;
}

.sidebar-footer {
  padding: 1rem 0.5rem;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  justify-content: center;
}

.sidebar:hover .sidebar-footer {
  justify-content: flex-start;
  padding: 1rem;
}

.logout-button {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: rgba(255, 255, 255, 0.6);
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 40px;
  width: 40px;
  justify-content: center;
  font-family: inherit;
}

.sidebar:hover .logout-button {
  width: 100%;
  justify-content: flex-start;
  padding: 0.75rem 1rem;
}

.logout-button:hover {
  background: rgba(255, 255, 255, 0.08);
  color: rgba(255, 255, 255, 0.9);
}

.logout-icon {
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.logout-text {
  display: none;
  font-size: 0.9rem;
  white-space: nowrap;
}

.sidebar:hover .logout-text {
  display: block;
}

/* Main content area */
.main-wrapper {
  flex: 1;
  margin-left: 64px;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.breadcrumb {
  font-size: 0.875rem;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #e0e0e0;
  background: white;
}

.breadcrumb-list {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.breadcrumb-link {
  color: #666;
  text-decoration: none;
  transition: color 0.2s ease;
}

.breadcrumb-link:hover {
  color: #333;
}

.breadcrumb-text {
  color: #333;
  font-weight: 500;
}

.breadcrumb-separator {
  color: #ccc;
}

.content {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Mobile responsive */
@media (max-width: 900px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 64px;
    flex-direction: row;
    border-right: none;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  }

  .sidebar:hover {
    width: 100%;
  }

  .sidebar-header {
    padding: 0 1.5rem;
    border-right: 1px solid rgba(255, 255, 255, 0.08);
    border-bottom: none;
    min-height: auto;
  }

  .sidebar-logo {
    font-size: 1rem;
  }

  .sidebar-nav {
    flex: 1;
    padding: 0;
    display: flex;
    flex-direction: row;
  }

  .nav-button {
    width: auto;
    padding: 0 1rem;
    min-height: 64px;
  }

  .sidebar:hover .nav-button {
    width: auto;
    padding: 0 1rem;
  }

  .nav-text {
    display: block;
  }

  .sidebar-footer {
    border-left: 1px solid rgba(255, 255, 255, 0.08);
    border-top: none;
    padding: 0 1rem;
  }

  .logout-button {
    width: auto;
    padding: 0;
    min-height: 64px;
  }

  .sidebar:hover .logout-button {
    width: auto;
    padding: 0;
  }

  .logout-text {
    display: block;
  }

  .main-wrapper {
    margin-left: 0;
    margin-top: 64px;
  }
}

/* Page transitions */
.page-enter-active,
.page-leave-active {
  transition: opacity 0.3s ease;
}

.page-enter-from {
  opacity: 0;
}

.page-leave-to {
  opacity: 0;
}
</style>
