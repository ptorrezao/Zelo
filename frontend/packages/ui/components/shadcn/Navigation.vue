<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { ref } from 'vue'

const router = useRouter()
const route = useRoute()
const expanded = ref(false)

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
  <nav class="nav-rail" @mouseenter="expanded = true" @mouseleave="expanded = false">
    <!-- Logo -->
    <div class="nav-logo">Zelo</div>

    <!-- Navigation Items -->
    <div class="nav-items">
      <button
        v-for="item in navItems"
        :key="item.path"
        :class="['nav-item', { active: isActive(item.path) }]"
        :title="item.label"
        @click="router.push(item.path)"
      >
        <span class="nav-icon">{{ item.icon }}</span>
        <span v-if="expanded" class="nav-label">{{ item.label }}</span>
      </button>
    </div>

    <!-- Footer User -->
    <div class="nav-footer">
      <button class="nav-user" :title="'Logout'" @click="handleLogout">
        <span class="user-icon">↗</span>
        <span v-if="expanded" class="user-label">Logout</span>
      </button>
    </div>
  </nav>
</template>

<style scoped>
.nav-rail {
  position: fixed;
  left: 0;
  top: 0;
  width: 64px;
  height: 100vh;
  background: #1a1a1a;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 1rem 0;
  z-index: 1000;
  transition: width 0.3s ease;
}

.nav-rail:hover {
  width: 240px;
  align-items: flex-start;
  padding: 1rem;
}

.nav-logo {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  font-weight: 700;
  color: #0066cc;
  margin-bottom: 2rem;
  cursor: pointer;
}

.nav-logo:hover {
  opacity: 0.8;
}

.nav-rail:hover .nav-logo {
  width: 100%;
  justify-content: flex-start;
  padding-left: 0.5rem;
  font-size: 1.25rem;
}

.nav-items {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
  width: 100%;
}

.nav-rail:hover .nav-items {
  align-items: flex-start;
}

.nav-item {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: rgba(255, 255, 255, 0.5);
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
  gap: 0.75rem;
}

.nav-rail:hover .nav-item {
  width: 100%;
  padding: 0.75rem 0.5rem;
  justify-content: flex-start;
}

.nav-label {
  display: none;
  font-size: 0.9rem;
  white-space: nowrap;
}

.nav-rail:hover .nav-label {
  display: block;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.08);
  color: rgba(255, 255, 255, 0.8);
}

.nav-item.active {
  background: #0066cc;
  color: white;
}

.nav-item.active::after {
  content: '';
  position: absolute;
  right: -8px;
  width: 4px;
  height: 20px;
  background: #0066cc;
  border-radius: 0 4px 4px 0;
}

.nav-icon {
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.nav-footer {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
  width: 100%;
}

.nav-rail:hover .nav-footer {
  align-items: flex-start;
}

.nav-user {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: rgba(255, 255, 255, 0.5);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 1.2rem;
  gap: 0.75rem;
}

.nav-rail:hover .nav-user {
  width: 100%;
  padding: 0.75rem 0.5rem;
  justify-content: flex-start;
}

.user-label {
  display: none;
  font-size: 0.9rem;
  white-space: nowrap;
}

.nav-rail:hover .user-label {
  display: block;
}

.nav-user:hover {
  background: rgba(255, 255, 255, 0.08);
  color: rgba(255, 255, 255, 0.8);
}

.user-icon {
  display: flex;
  align-items: center;
  justify-content: center;
}

@media (max-width: 900px) {
  .nav-rail {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 64px;
    flex-direction: row;
    border-right: none;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    padding: 0 1rem;
  }

  .nav-rail:hover {
    width: 100%;
    padding: 0 1rem;
  }

  .nav-logo {
    margin-bottom: 0;
    margin-right: 2rem;
    width: auto;
    font-size: 1rem;
  }

  .nav-rail:hover .nav-logo {
    font-size: 1rem;
    padding-left: 0;
  }

  .nav-items {
    flex-direction: row;
    gap: 1rem;
    flex: 1;
    justify-content: flex-start;
  }

  .nav-rail:hover .nav-items {
    align-items: center;
  }

  .nav-item {
    width: 40px;
    height: 40px;
  }

  .nav-rail:hover .nav-item {
    width: 40px;
    padding: 0;
  }

  .nav-item.active::after {
    content: '';
    position: absolute;
    bottom: -8px;
    right: auto;
    width: 20px;
    height: 4px;
    background: #0066cc;
    border-radius: 0 0 4px 4px;
  }

  .nav-footer {
    margin-left: auto;
    flex-direction: row;
  }

  .nav-rail:hover .nav-footer {
    align-items: center;
  }

  .nav-user {
    width: 40px;
    height: 40px;
  }

  .nav-rail:hover .nav-user {
    width: 40px;
    padding: 0;
  }
}
</style>
