<template>
  <main class="z-workspace">
    <slot />
  </main>
</template>

<style scoped>
.z-workspace {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-3);
  flex: 1;
  min-width: 0;
  min-height: 0;
  overflow-y: auto;
}
</style>
