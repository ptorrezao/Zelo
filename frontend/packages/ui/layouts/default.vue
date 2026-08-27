<script setup lang="ts">
// Unico layout das apps Zelo. O rail de modulos e sempre o primeiro
// elemento; a pagina preenche o resto com Container, opcionalmente
// precedido de um sidebar quando precisa de uma lista a esquerda.
// O flex aceita as duas composicoes sem variantes de layout.
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Sidebar from '../components/shadcn/Sidebar.vue'

const route = useRoute()

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
</script>

<template>
  <div class="z-shell">
    <Sidebar />
    <div class="z-shell__wrapper">
      <nav class="z-breadcrumb" aria-label="Breadcrumb">
        <ol class="z-breadcrumb__list">
          <li v-for="(item, index) in breadcrumbs" :key="index" class="z-breadcrumb__item">
            <a v-if="!item.isLast" :href="item.href" class="z-breadcrumb__link">
              {{ item.label }}
            </a>
            <span v-else class="z-breadcrumb__text">
              {{ item.label }}
            </span>
            <span v-if="index < breadcrumbs.length - 1" class="z-breadcrumb__separator" aria-hidden="true">/</span>
          </li>
        </ol>
      </nav>
      <div class="z-shell__content">
        <Transition name="page" mode="out-in">
          <slot :key="route.path" />
        </Transition>
      </div>
    </div>
  </div>
</template>

<style scoped>
.z-shell {
  display: flex;
  gap: var(--z-space-3);
  height: calc(100vh - 64px);
  margin-left: 64px;
  padding: var(--z-space-3);
  box-sizing: border-box;
  background: var(--z-color-page);
}

.z-shell__wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.z-shell__content {
  flex: 1;
  display: flex;
  gap: var(--z-space-3);
  min-width: 0;
  min-height: 0;
}

@media (max-width: 900px) {
  .z-shell {
    /* o rail passa a barra de topo, portanto empilha-se sobre o conteudo */
    flex-direction: column;
    height: auto;
    min-height: 100vh;
    margin-left: 0;
    margin-top: 64px;
  }
}

/* Breadcrumb */
.z-breadcrumb {
  font-size: var(--z-font-size-sm);
  margin-bottom: var(--z-space-3);
}

.z-breadcrumb__list {
  display: flex;
  align-items: center;
  gap: var(--z-space-2);
  margin: 0;
  padding: 0;
  list-style: none;
}

.z-breadcrumb__item {
  display: flex;
  align-items: center;
  gap: var(--z-space-2);
}

.z-breadcrumb__link {
  color: var(--z-color-text-muted);
  text-decoration: none;
  transition: color 0.2s ease;
}

.z-breadcrumb__link:hover {
  color: var(--z-color-text);
}

.z-breadcrumb__text {
  color: var(--z-color-text);
  font-weight: 500;
}

.z-breadcrumb__separator {
  color: var(--z-color-border);
}

/* Page Transitions */
.page-enter-active,
.page-leave-active {
  transition: opacity 0.8s ease;
}

.page-enter-from {
  opacity: 0;
}

.page-leave-to {
  opacity: 0;
}
</style>
