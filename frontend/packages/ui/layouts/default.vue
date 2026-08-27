<script setup lang="ts">
// Unico layout das apps Zelo. O rail de modulos e sempre o primeiro
// elemento; a pagina preenche o resto com ZWorkspace, opcionalmente
// precedido de ZSidePanel quando precisa de uma lista a esquerda.
// O flex aceita as duas composicoes sem variantes de layout.
</script>

<template>
  <div class="z-shell">
    <ZNavRail />
    <slot />
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
</style>
