<script setup lang="ts">
export interface BreadcrumbItem {
  label: string
  href?: string
}

defineProps<{
  items: BreadcrumbItem[]
}>()
</script>

<template>
  <nav class="breadcrumb" aria-label="Breadcrumb">
    <ol class="breadcrumb__list">
      <li v-for="(item, index) in items" :key="index" class="breadcrumb__item">
        <a v-if="item.href" :href="item.href" class="breadcrumb__link">
          {{ item.label }}
        </a>
        <span v-else class="breadcrumb__text">
          {{ item.label }}
        </span>
        <span v-if="index < items.length - 1" class="breadcrumb__separator" aria-hidden="true">/</span>
      </li>
    </ol>
  </nav>
</template>

<style scoped>
.breadcrumb {
  font-size: var(--z-font-size-sm);
}

.breadcrumb__list {
  display: flex;
  align-items: center;
  gap: var(--z-space-2);
  margin: 0;
  padding: 0;
  list-style: none;
}

.breadcrumb__item {
  display: flex;
  align-items: center;
  gap: var(--z-space-2);
}

.breadcrumb__link {
  color: var(--z-color-text-muted);
  text-decoration: none;
  transition: color 0.2s ease;
}

.breadcrumb__link:hover {
  color: var(--z-color-text);
}

.breadcrumb__text {
  color: var(--z-color-text);
  font-weight: 500;
}

.breadcrumb__separator {
  color: var(--z-color-border);
}
</style>
