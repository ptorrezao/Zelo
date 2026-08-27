<template>
  <div class="z-table__scroll">
    <table class="z-table">
      <slot />
    </table>
  </div>
</template>

<style scoped>
/* O contentor rola sozinho para a pagina nunca ganhar barra horizontal. */
.z-table__scroll {
  overflow-x: auto;
}

.z-table {
  width: 100%;
  border-collapse: collapse;
  font-size: var(--z-font-size-sm);
}

.z-table :deep(th) {
  text-align: left;
  font-weight: 400;
  color: var(--z-color-text-muted);
}

.z-table :deep(td),
.z-table :deep(th) {
  padding: var(--z-space-3) 0;
  border-bottom: var(--z-border-subtle);
}

.z-table :deep(tr:last-child td) {
  border-bottom: 0;
}

/* Colunas de numeros alinham a direita e com digitos de largura fixa. */
.z-table :deep(td.z-num),
.z-table :deep(th.z-num) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>
