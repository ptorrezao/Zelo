<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    name: string
    /** Imagem a mostrar. Sem ela, ou se falhar, ficam as iniciais. */
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

// Com render no servidor o pedido da imagem acontece antes de o listener
// existir, portanto pergunta-se ao elemento em vez de esperar pelo evento.
function checkLoaded() {
  const el = imgEl.value
  if (el && el.complete && el.naturalWidth === 0) { failed.value = true }
}

onMounted(checkLoaded)

const showImage = computed(() => Boolean(props.src) && !failed.value)
</script>

<template>
  <span class="z-avatar" :class="`z-avatar--${size}`">
    <img
      v-if="showImage"
      ref="imgEl"
      class="z-avatar__image"
      :src="src"
      :alt="name"
      @error="failed = true"
      @load="checkLoaded"
    >
    <span v-else class="z-avatar__initials">{{ initials }}</span>

    <span v-if="online" class="z-avatar__dot" :title="`${name} está online`" />
  </span>
</template>

<style scoped>
.z-avatar {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: none;
  border-radius: 50%;
  background: var(--z-color-surface);
  border: var(--z-border-subtle);
  color: var(--z-color-text-muted);
  font-weight: 600;
  user-select: none;
}

.z-avatar--sm { width: 28px; height: 28px; font-size: var(--z-font-size-xs); }
.z-avatar--md { width: 36px; height: 36px; font-size: var(--z-font-size-xs); }
.z-avatar--lg { width: 44px; height: 44px; font-size: var(--z-font-size-sm); }

/* contain e nao cover: um logotipo cortado deixa de se reconhecer.
   O redondo vai na imagem e nao no contentor, senao o ponto de estado,
   que assenta na borda, seria cortado. */
.z-avatar__image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  padding: 15%;
  box-sizing: border-box;
  border-radius: 50%;
}

.z-avatar__dot {
  position: absolute;
  right: -1px;
  bottom: -1px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--z-series-3);
  /* anel na cor da superficie, para o ponto ler sobre o avatar */
  box-shadow: 0 0 0 2px var(--z-color-bg);
}
</style>
