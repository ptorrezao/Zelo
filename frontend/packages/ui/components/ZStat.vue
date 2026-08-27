<script setup lang="ts">
withDefaults(defineProps<{ label: string, value?: string, size?: 'md' | 'lg' }>(), {
  size: 'md',
})
</script>

<template>
  <div class="z-stat">
    <div class="z-stat__label">{{ label }}</div>
    <div class="z-stat__value" :class="`z-stat__value--${size}`">
      <slot>{{ value }}</slot>
    </div>
  </div>
</template>

<style scoped>
.z-stat__label {
  margin-bottom: var(--z-space-1);
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.z-stat__value {
  font-weight: 600;
  /* O valor nunca parte a meio: prefere-se reduzir a coluna a quebrar
     "2 160 kg" em duas linhas. */
  white-space: nowrap;
}

.z-stat__value--md {
  font-size: var(--z-font-size-base);
}

.z-stat__value--lg {
  font-size: var(--z-font-size-lg);
}
</style>
