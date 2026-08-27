<script setup lang="ts">
withDefaults(defineProps<{ placeholder?: string }>(), { placeholder: 'Procurar...' })
const model = defineModel<string>()
const id = useId()
</script>

<template>
  <div class="z-search">
    <svg class="z-search__icon" viewBox="0 0 16 16" aria-hidden="true">
      <circle cx="7" cy="7" r="4.5" fill="none" stroke="currentColor" stroke-width="1.5" />
      <path d="M10.5 10.5 14 14" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
    </svg>
    <input :id="id" v-model="model" class="z-search__input" type="search" :placeholder="placeholder">
  </div>
</template>

<style scoped>
.z-search {
  display: flex;
  align-items: center;
  gap: var(--z-space-2);
  padding: var(--z-space-2) var(--z-space-3);
}

.z-search__icon {
  width: 16px;
  height: 16px;
  flex: none;
  color: var(--z-color-text-subtle);
}

.z-search__input {
  flex: 1;
  min-width: 0;
  border: 0;
  background: none;
  font: inherit;
  font-size: var(--z-font-size-sm);
  outline-offset: 4px;
}
</style>
