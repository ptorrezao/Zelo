<script setup lang="ts">
import { onBeforeUnmount, watch } from 'vue'
import { cn } from '../../lib/utils'

interface Props {
  open: boolean
  class?: string
  side?: 'right' | 'left'
}

const props = withDefaults(defineProps<Props>(), {
  side: 'right',
})

const emit = defineEmits<{ 'update:open': [value: boolean] }>()

function close() {
  emit('update:open', false)
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') close()
}

watch(
  () => props.open,
  (open) => {
    if (typeof document === 'undefined') return
    document.body.style.overflow = open ? 'hidden' : ''
    if (open) {
      window.addEventListener('keydown', handleKeydown)
    } else {
      window.removeEventListener('keydown', handleKeydown)
    }
  },
)

onBeforeUnmount(() => {
  if (typeof document === 'undefined') return
  document.body.style.overflow = ''
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <Teleport to="body">
    <Transition name="sheet-overlay">
      <div v-if="open" class="fixed inset-0 z-50 bg-black/50" @click="close" />
    </Transition>
    <Transition :name="side === 'right' ? 'sheet-slide-right' : 'sheet-slide-left'">
      <div
        v-if="open"
        :class="[
          'fixed inset-y-0 z-50 flex w-full max-w-md flex-col gap-4 border-border bg-background p-6 shadow-lg',
          side === 'right' ? 'right-0 border-l' : 'left-0 border-r',
          props.class,
        ]"
      >
        <slot :close="close" />
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.sheet-overlay-enter-active,
.sheet-overlay-leave-active {
  transition: opacity 0.2s ease;
}
.sheet-overlay-enter-from,
.sheet-overlay-leave-to {
  opacity: 0;
}

.sheet-slide-right-enter-active,
.sheet-slide-right-leave-active,
.sheet-slide-left-enter-active,
.sheet-slide-left-leave-active {
  transition: transform 0.25s ease;
}
.sheet-slide-right-enter-from,
.sheet-slide-right-leave-to {
  transform: translateX(100%);
}
.sheet-slide-left-enter-from,
.sheet-slide-left-leave-to {
  transform: translateX(-100%);
}
</style>
