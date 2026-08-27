<script setup lang="ts">
import SCard from './shadcn/Card.vue'
import SCardHeader from './shadcn/CardHeader.vue'
import SCardContent from './shadcn/CardContent.vue'

defineProps<{ title?: string }>()
</script>

<template>
  <SCard class="z-panel">
    <SCardHeader v-if="title || $slots.action" class="z-panel__header">
      <div class="z-panel__header-content">
        <h2 v-if="title" class="z-panel__title">{{ title }}</h2>
        <div v-if="$slots.action" class="z-panel__action">
          <slot name="action" />
        </div>
      </div>
    </SCardHeader>
    <SCardContent class="z-panel__content">
      <slot />
    </SCardContent>
  </SCard>
</template>

<style scoped>
.z-panel__header {
  padding: var(--z-space-6);
  border-bottom: var(--z-border-subtle);
}

.z-panel__header-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--z-space-4);
  width: 100%;
}

.z-panel__title {
  margin: 0;
  font-size: var(--z-font-size-base);
  font-weight: 600;
}

.z-panel__content {
  padding: var(--z-space-6);
}
</style>
