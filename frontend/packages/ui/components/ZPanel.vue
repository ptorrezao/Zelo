<script setup lang="ts">
defineProps<{ title?: string }>()
</script>

<template>
  <section class="z-panel">
    <header v-if="title || $slots.action" class="z-panel__header">
      <h2 v-if="title" class="z-panel__title">{{ title }}</h2>
      <div v-if="$slots.action" class="z-panel__action">
        <slot name="action" />
      </div>
    </header>
    <slot />
  </section>
</template>

<style scoped>
.z-panel {
  padding: var(--z-space-6);
  background: var(--z-color-bg);
  border-radius: var(--z-radius-lg);
}

.z-panel__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-4);
  margin-bottom: var(--z-space-6);
}

.z-panel__title {
  margin: 0;
  font-size: var(--z-font-size-base);
  font-weight: 600;
}
</style>
