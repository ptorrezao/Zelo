<script setup lang="ts">
defineProps<{ options: { value: string, label: string }[] }>()
const model = defineModel<string>({ required: true })
</script>

<template>
  <div class="z-segctl" role="group">
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      class="z-segctl__option"
      :class="{ 'z-segctl__option--active': option.value === model }"
      :aria-pressed="option.value === model"
      @click="model = option.value"
    >
      {{ option.label }}
    </button>
  </div>
</template>

<style scoped>
.z-segctl {
  display: inline-flex;
  gap: var(--z-space-1);
}

.z-segctl__option {
  padding: var(--z-space-1) var(--z-space-2);
  border: 0;
  border-radius: var(--z-radius);
  background: none;
  font: inherit;
  font-size: var(--z-font-size-xs);
  color: var(--z-color-text-subtle);
  cursor: pointer;
}

.z-segctl__option:hover {
  background: var(--z-color-surface);
}

.z-segctl__option--active {
  background: var(--z-color-surface);
  color: var(--z-color-text);
  font-weight: 600;
}
</style>
