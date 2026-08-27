<script setup lang="ts">
// <details> da o colapso e a semantica sem javascript nem estado proprio.
withDefaults(defineProps<{ label: string, open?: boolean }>(), { open: true })
</script>

<template>
  <details class="z-group" :open="open">
    <summary class="z-group__summary">
      <span class="z-group__label"><slot name="label">{{ label }}</slot></span>
      <svg class="z-group__chevron" viewBox="0 0 12 12" aria-hidden="true">
        <path d="M3 4.5 6 7.5 9 4.5" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
      </svg>
    </summary>
    <slot />
  </details>
</template>

<style scoped>
.z-group__summary {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-2);
  padding: var(--z-space-3) var(--z-space-4);
  cursor: pointer;
  list-style: none;
  color: var(--z-color-text-subtle);
}

.z-group__summary::-webkit-details-marker {
  display: none;
}

.z-group__label {
  display: flex;
  align-items: center;
  gap: var(--z-space-1);
  font-size: var(--z-font-size-xs);
  font-weight: 600;
  text-transform: var(--z-label-transform);
  letter-spacing: var(--z-label-spacing);
}

.z-group__chevron {
  width: 12px;
  height: 12px;
  flex: none;
  transform: rotate(180deg);
}

.z-group:not([open]) .z-group__chevron {
  transform: rotate(0deg);
}
</style>
