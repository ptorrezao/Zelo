<script setup lang="ts">
export interface Props {
  title: string
  subtitle?: string
  avatar?: string
  avatarName?: string
}

withDefaults(defineProps<Props>(), {
  subtitle: '',
})
</script>

<template>
  <div class="page-header">
    <div v-if="avatar || avatarName" class="page-header__avatar">
      <ZAvatar
        v-if="avatarName"
        :name="avatarName"
        size="lg"
      />
      <img v-else :src="avatar" :alt="title" class="page-header__image" />
    </div>
    <div class="page-header__content">
      <h1 class="page-header__title">{{ title }}</h1>
      <p v-if="subtitle" class="page-header__subtitle">{{ subtitle }}</p>
    </div>
    <div class="page-header__actions">
      <slot name="actions" />
    </div>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  gap: var(--z-space-6);
}

.page-header__avatar {
  flex: none;
}

.page-header__image {
  width: 80px;
  height: 80px;
  border-radius: var(--z-radius-lg);
  object-fit: cover;
}

.page-header__content {
  flex: 1;
}

.page-header__title {
  margin: 0 0 var(--z-space-1);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
  color: var(--z-color-text);
}

.page-header__subtitle {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.page-header__actions {
  flex: none;
}
</style>
