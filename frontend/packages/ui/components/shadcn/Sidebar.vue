<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { computed } from 'vue'

const router = useRouter()
const route = useRoute()

const isActive = (path: string) => {
  return route.path.startsWith(path)
}

const handleLogout = () => {
  router.push('/login')
}

const navItems = [
  { label: 'Início', path: '/', icon: '⌂' },
  { label: 'Auto', path: '/auto', icon: '◆' },
  { label: 'Inventário', path: '/inventory', icon: '≡' },
]
</script>

<template>
  <div class="sidebar-container">
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="sidebar-logo">Z</div>
      </div>

      <nav class="sidebar-nav">
        <div class="nav-group">
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
        </div>
      </nav>

      <div class="sidebar-footer">
        <button class="logout-button" @click="handleLogout" title="Logout">
          <span class="logout-icon">↗</span>
          <span class="logout-text">Logout</span>
        </button>
      </div>
    </aside>

    <div class="sidebar-spacer"></div>
  </div>
</template>

<style scoped>
.sidebar-container {
  display: contents;
}

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
}

.nav-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0 0.5rem;
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

.sidebar-spacer {
  position: fixed;
  left: 0;
  top: 0;
  width: 64px;
  height: 0;
}

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

  .sidebar:hover .sidebar-header {
    justify-content: flex-start;
  }

  .sidebar-logo {
    font-size: 1rem;
  }

  .sidebar:hover .sidebar-logo {
    font-size: 1rem;
    width: auto;
  }

  .sidebar-nav {
    flex: 1;
    padding: 0;
    display: flex;
  }

  .nav-group {
    flex-direction: row;
    gap: 0;
    padding: 0;
    flex: 1;
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

  .sidebar:hover .sidebar-footer {
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
}
</style>
