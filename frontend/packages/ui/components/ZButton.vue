<script setup lang="ts">
withDefaults(defineProps<{ variant?: 'primary' | 'secondary' }>(), {
  variant: 'secondary',
})
</script>

<template>
  <button class="z-button" :class="`z-button--${variant}`">
    <slot />
  </button>
</template>

<style scoped>
.z-button {
  padding: var(--z-space-2) var(--z-space-4);
  border-radius: var(--z-radius);
  border: var(--z-border);
  font: inherit;
  font-size: var(--z-font-size-sm);
  cursor: pointer;
}

.z-button--primary {
  background: var(--z-color-accent);
  border-color: var(--z-color-accent);
  color: var(--z-color-accent-text);
}

.z-button--secondary {
  background: var(--z-color-bg);
  color: var(--z-color-text);
}
</style>
