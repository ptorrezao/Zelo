<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import SAvatar from './Avatar.vue'

const router = useRouter()
const route = useRoute()

const isActive = (path: string) => {
  return route.path.startsWith(path)
}

const handleLogout = () => {
  router.push('/login')
}

const navItems = [
  { label: 'Início', path: '/', icon: '🏠' },
  { label: 'Auto', path: '/auto', icon: '🚗' },
  { label: 'Inventário', path: '/inventory', icon: '📦' },
]
</script>

<template>
  <nav class="nav-rail">
    <div class="nav-header">
      <h1 class="nav-logo">Zelo</h1>
    </div>

    <div class="nav-items">
      <button
        v-for="item in navItems"
        :key="item.path"
        :class="['nav-item', { active: isActive(item.path) }]"
        @click="router.push(item.path)"
      >
        <span class="nav-icon">{{ item.icon }}</span>
        <span class="nav-label">{{ item.label }}</span>
      </button>
    </div>

    <div class="nav-footer">
      <div class="nav-user">
        <SAvatar initials="U" class="user-avatar" />
        <div class="user-info">
          <span class="user-email">user@example.com</span>
          <button class="logout-btn" @click="handleLogout">Logout</button>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.nav-rail {
  display: flex;
  flex-direction: column;
  width: 240px;
  background: #1f1f1f;
  color: white;
  padding: 1.5rem 1rem;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

.nav-header {
  padding-bottom: 1.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  margin-bottom: 1.5rem;
}

.nav-logo {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  color: #0066cc;
}

.nav-items {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem;
  border: none;
  border-radius: 0.5rem;
  background: transparent;
  color: rgba(255, 255, 255, 0.7);
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: left;
  font-size: 0.95rem;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.nav-item.active {
  background: #0066cc;
  color: white;
}

.nav-icon {
  flex: none;
  font-size: 1.25rem;
}

.nav-label {
  flex: 1;
}

.nav-footer {
  padding-top: 1.5rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.nav-user {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border-radius: 0.5rem;
  background: rgba(0, 102, 204, 0.1);
}

:deep(.user-avatar) {
  flex: none;
  width: 32px;
  height: 32px;
}

.user-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.user-email {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.8);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.logout-btn {
  font-size: 0.75rem;
  color: #0066cc;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  text-decoration: none;
  transition: color 0.2s ease;
}

.logout-btn:hover {
  color: #0052a3;
  text-decoration: underline;
}

@media (max-width: 900px) {
  .nav-rail {
    width: 100%;
    flex-direction: row;
    padding: 1rem;
    margin-bottom: 1rem;
  }

  .nav-header {
    padding-right: 1.5rem;
    padding-bottom: 0;
    border-right: 1px solid rgba(255, 255, 255, 0.1);
    border-bottom: none;
    margin-right: 1.5rem;
    margin-bottom: 0;
  }

  .nav-logo {
    font-size: 1.25rem;
  }

  .nav-items {
    flex-direction: row;
    gap: 0.5rem;
    flex: 1;
  }

  .nav-item {
    justify-content: center;
    width: 50px;
    padding: 0.75rem;
  }

  .nav-label {
    display: none;
  }

  .nav-footer {
    padding-left: 1.5rem;
    padding-top: 0;
    border-left: 1px solid rgba(255, 255, 255, 0.1);
    border-top: none;
  }

  .nav-user {
    flex-direction: column;
  }

  .user-info {
    display: none;
  }
}
</style>
