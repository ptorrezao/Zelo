<script setup lang="ts">
defineProps<{ label: string }>()
const model = defineModel<string>()
const id = useId()
</script>

<template>
  <div class="z-field">
    <label class="z-field__label" :for="id">{{ label }}</label>
    <input :id="id" v-model="model" class="z-field__input">
  </div>
</template>

<style scoped>
.z-field {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-1);
}

.z-field__label {
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.z-field__input {
  padding: var(--z-space-2) var(--z-space-3);
  border: var(--z-border);
  border-radius: var(--z-radius);
  font: inherit;
}
</style>
