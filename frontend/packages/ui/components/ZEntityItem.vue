<script setup lang="ts">
withDefaults(
  defineProps<{
    title: string
    subtitle?: string
    /** Imagem do avatar. Sem ela ficam as iniciais do titulo. */
    image?: string
    status?: string
    alert?: boolean
    online?: boolean
    selected?: boolean
  }>(),
  { alert: false, online: false, selected: false },
)
</script>

<template>
  <button
    type="button"
    class="z-entity"
    :class="{ 'z-entity--selected': selected }"
    :aria-current="selected ? 'true' : undefined"
  >
    <ZAvatar :name="title" :src="image" size="md" :online="online" />

    <span class="z-entity__text">
      <span class="z-entity__title">{{ title }}</span>
      <span v-if="subtitle" class="z-entity__subtitle">{{ subtitle }}</span>
    </span>

    <ZStatusBadge v-if="status" :label="status" :alert="alert" />
  </button>
</template>

<style scoped>
.z-entity {
  display: flex;
  align-items: center;
  gap: var(--z-space-3);
  width: 100%;
  padding: var(--z-space-2) var(--z-space-4);
  border: 0;
  background: none;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.z-entity:hover {
  background: var(--z-color-surface);
}

.z-entity--selected {
  background: var(--z-color-surface);
}

.z-entity__text {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.z-entity__title,
.z-entity__subtitle {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.z-entity__title {
  font-size: var(--z-font-size-sm);
  font-weight: 600;
}

.z-entity__subtitle {
  font-size: var(--z-font-size-xs);
  color: var(--z-color-text-muted);
}
</style>
