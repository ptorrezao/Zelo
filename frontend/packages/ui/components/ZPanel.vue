<script setup lang="ts">
interface Props {
  title?: string
  class?: string
}

defineProps<Props>()
</script>

<template>
  <div class="z-panel" :class="$attrs.class">
    <div v-if="title" class="z-panel__header">
      <h3 class="z-panel__title">{{ title }}</h3>
      <div class="z-panel__action">
        <slot name="action" />
      </div>
    </div>
    <div class="z-panel__content">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.z-panel {
  padding: var(--z-space-4);
  border: 1px solid var(--z-color-border);
  border-radius: var(--z-radius-md);
  background: var(--z-color-background-secondary);
}

.z-panel__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--z-space-4);
  padding-bottom: var(--z-space-4);
  border-bottom: 1px solid var(--z-color-border);
}

.z-panel__title {
  margin: 0;
  font-size: var(--z-font-size-base);
  font-weight: 600;
  color: var(--z-color-text);
}

.z-panel__action {
  flex: none;
}

.z-panel__content {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-3);
}
</style>
