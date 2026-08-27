<script setup lang="ts">
export interface Props {
  columns?: number
}

withDefaults(defineProps<Props>(), {
  columns: 2,
})
</script>

<template>
  <div class="detail-grid" :style="{ '--grid-cols': columns }">
    <slot />
  </div>
</template>

<style scoped>
.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--z-space-3);
  align-items: start;
}

@media (max-width: 768px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>
