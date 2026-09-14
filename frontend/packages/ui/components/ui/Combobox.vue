<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { cn } from '../../lib/utils'

interface Props {
  modelValue: string
  options: string[]
  placeholder?: string
  class?: string
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'Selecionar...',
  disabled: false,
})

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const open = ref(false)
const root = ref<HTMLElement>()

const filteredOptions = computed(() => {
  const query = props.modelValue.trim().toLowerCase()
  if (!query) return props.options
  return props.options.filter(o => o.toLowerCase().includes(query))
})

function onInput(event: Event) {
  emit('update:modelValue', (event.target as HTMLInputElement).value)
  open.value = true
}

function select(value: string) {
  emit('update:modelValue', value)
  open.value = false
}

function handleClickOutside(event: MouseEvent) {
  if (root.value && !root.value.contains(event.target as Node)) {
    open.value = false
  }
}

onMounted(() => document.addEventListener('click', handleClickOutside))
onBeforeUnmount(() => document.removeEventListener('click', handleClickOutside))
</script>

<template>
  <div ref="root" class="relative">
    <input
      type="text"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :class="cn(
        'flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
        props.class,
      )"
      @input="onInput"
      @focus="open = true"
    >

    <div
      v-if="open && filteredOptions.length > 0"
      class="absolute z-20 mt-1 max-h-60 w-full overflow-y-auto rounded-md border border-border bg-popover p-1 text-popover-foreground shadow-md"
    >
      <button
        v-for="option in filteredOptions"
        :key="option"
        type="button"
        :class="[
          'flex w-full items-center justify-between rounded-sm px-2 py-1.5 text-left text-sm hover:bg-accent',
          option === modelValue ? 'bg-accent font-medium' : '',
        ]"
        @click="select(option)"
      >
        {{ option }}
      </button>
    </div>
  </div>
</template>
