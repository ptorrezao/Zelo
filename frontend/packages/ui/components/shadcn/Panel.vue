<script setup lang="ts">
interface Props {
  title?: string
  class?: string
}

defineProps<Props>()
</script>

<template>
  <div class="panel" :class="$attrs.class">
    <div v-if="title" class="panel-header">
      <h3 class="panel-title">{{ title }}</h3>
      <div class="panel-action">
        <slot name="action" />
      </div>
    </div>
    <div class="panel-content">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.panel {
  padding: 1.5rem;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 0.5rem;
  background: white;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.panel-title {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
}

.panel-action {
  flex: none;
}

.panel-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
