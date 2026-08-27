<template>
  <p class="z-section-label">
    <slot />
  </p>
</template>

<style scoped>
.z-section-label {
  margin: 0 0 var(--z-space-3);
  font-size: var(--z-font-size-xs);
  text-transform: var(--z-label-transform);
  letter-spacing: var(--z-label-spacing);
  color: var(--z-color-text-subtle);
}
</style>
