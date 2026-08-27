<script setup lang="ts">
// Unico layout das apps Zelo. O rail de modulos e sempre o primeiro
// elemento; a pagina preenche o resto com ZWorkspace, opcionalmente
// precedido de ZSidePanel quando precisa de uma lista a esquerda.
// O flex aceita as duas composicoes sem variantes de layout.
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <div class="z-shell">
    <ZNavRail />
    <Transition name="page" mode="out-in">
      <slot :key="route.path" />
    </Transition>
  </div>
</template>

<style scoped>
.z-shell {
  display: flex;
  gap: var(--z-space-3);
  height: 100vh;
  padding: var(--z-space-3);
  box-sizing: border-box;
  background: var(--z-color-page);
}

@media (max-width: 900px) {
  .z-shell {
    /* o rail passa a barra de topo, portanto empilha-se sobre o conteudo */
    flex-direction: column;
    height: auto;
    min-height: 100vh;
  }
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
