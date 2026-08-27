<script setup lang="ts">
defineProps<{ title?: string }>()
</script>

<template>
  <section class="z-card">
    <h2 v-if="title" class="z-card__title">{{ title }}</h2>
    <slot />
  </section>
</template>

<style scoped>
.z-card {
  padding: var(--z-space-4);
  background: var(--z-color-surface);
  border: var(--z-border);
  border-radius: var(--z-radius);
}

.z-card__title {
  margin: 0 0 var(--z-space-3);
  font-size: var(--z-font-size-lg);
}
</style>
