<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import SAvatar from './shadcn/Avatar.vue'

const props = withDefaults(
  defineProps<{
    name: string
    src?: string
    size?: 'sm' | 'md' | 'lg'
    online?: boolean
  }>(),
  { size: 'md', online: false },
)

const initials = computed(() =>
  props.name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part[0])
    .join('')
    .toUpperCase(),
)

const failed = ref(false)
const imgEl = ref<HTMLImageElement | null>(null)

watch(() => props.src, () => { failed.value = false })

function checkLoaded() {
  const el = imgEl.value
  if (el && el.complete && el.naturalWidth === 0) { failed.value = true }
}

onMounted(checkLoaded)

const showImage = computed(() => Boolean(props.src) && !failed.value)

const sizeClasses = {
  sm: 'h-7 w-7 text-xs',
  md: 'h-9 w-9 text-xs',
  lg: 'h-11 w-11 text-sm',
}
</script>

<template>
  <span class="z-avatar relative" :class="sizeClasses[size]">
    <SAvatar
      :initials="initials"
      :src="showImage ? src : undefined"
      :alt="name"
      :class="sizeClasses[size]"
    />

    <span v-if="online" class="z-avatar__dot" :title="`${name} está online`" />
  </span>
</template>

<style scoped>
.z-avatar__dot {
  position: absolute;
  right: -1px;
  bottom: -1px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--z-series-3);
  box-shadow: 0 0 0 2px var(--z-color-bg);
}
</style>
