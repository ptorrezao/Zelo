<script setup lang="ts">
import { useId } from 'vue'
import SInput from './shadcn/Input.vue'

interface Props {
  label?: string
  placeholder?: string
  type?: string
  disabled?: boolean
}

defineProps<Props>()
const model = defineModel<string>()
const id = useId()
</script>

<template>
  <div class="z-field">
    <label v-if="label" class="z-field__label" :for="id">{{ label }}</label>
    <SInput
      :id="id"
      :model-value="model"
      :placeholder="placeholder"
      :type="type"
      :disabled="disabled"
      @update:model-value="model = $event"
    />
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
</style>
