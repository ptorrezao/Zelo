<template>
  <aside class="z-side">
    <slot />
  </aside>
</template>

<style scoped>
.z-side {
  display: flex;
  flex-direction: column;
  flex: none;
  width: 260px;
  min-height: 0;
  background: var(--z-color-bg);
  border-radius: var(--z-radius-lg);
}

@media (max-width: 900px) {
  .z-side {
    display: none;
  }
}
</style>
