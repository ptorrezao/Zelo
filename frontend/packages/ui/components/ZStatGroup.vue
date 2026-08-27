<script setup lang="ts">
// Agrupa varios ZStat. divided desenha o separador vertical entre eles.
withDefaults(defineProps<{ columns?: number, divided?: boolean }>(), {
  columns: 4,
  divided: false,
})
</script>

<template>
  <div
    class="z-stat-group"
    :class="{ 'z-stat-group--divided': divided }"
    :style="{ '--z-stat-columns': columns }"
  >
    <slot />
  </div>
</template>

<style scoped>
.z-stat-group {
  display: grid;
  grid-template-columns: repeat(var(--z-stat-columns), minmax(0, 1fr));
  gap: var(--z-space-4);
}

.z-stat-group--divided > :slotted(*) + :slotted(*) {
  padding-left: var(--z-space-4);
  border-left: var(--z-border-subtle);
}
</style>
