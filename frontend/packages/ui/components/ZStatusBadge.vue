<script setup lang="ts">
// alert marca o estado que precisa de atencao. A cor nunca vai sozinha:
// o rotulo esta sempre presente e o ponto e apenas reforco.
withDefaults(defineProps<{ label: string, alert?: boolean }>(), { alert: false })
</script>

<template>
  <span class="z-badge" :class="{ 'z-badge--alert': alert }">
    <span v-if="alert" class="z-badge__mark" aria-hidden="true" />
    {{ label }}
  </span>
</template>

<style scoped>
.z-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--z-space-1);
  font-size: var(--z-font-size-xs);
  text-transform: var(--z-label-transform);
  letter-spacing: var(--z-label-spacing);
  color: var(--z-color-text-subtle);
  white-space: nowrap;
}

.z-badge--alert {
  color: var(--z-color-text);
  font-weight: 600;
}

.z-badge__mark {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--z-color-warning);
}
</style>
