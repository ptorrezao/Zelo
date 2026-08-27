<script setup lang="ts">
import SButton from './shadcn/Button.vue'

interface Props {
  variant?: 'primary' | 'secondary'
  size?: 'default' | 'sm' | 'lg'
  disabled?: boolean
}

withDefaults(defineProps<Props>(), {
  variant: 'secondary',
  size: 'default',
})
</script>

<template>
  <SButton
    :variant="variant === 'primary' ? 'default' : 'secondary'"
    :size="size"
    :disabled="disabled"
  >
    <slot />
  </SButton>
</template>
