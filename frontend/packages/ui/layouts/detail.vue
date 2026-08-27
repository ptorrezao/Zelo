<script setup lang="ts">
// Standard detail page layout used across all apps
// Provides consistent header + content structure
</script>

<template>
  <div class="detail-layout">
    <slot />
  </div>
</template>

<style scoped>
.detail-layout {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-6);
}
</style>
