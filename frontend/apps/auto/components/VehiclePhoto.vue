<script setup lang="ts">
const props = defineProps<{ src?: string, alt: string }>()

// Um ficheiro em falta nao pode deixar um icone de imagem partida no ecra:
// cai no marcador de posicao. O reset e preciso porque trocar de veiculo
// reutiliza o mesmo <img>.
const failed = ref(false)
const photoEl = ref<HTMLImageElement | null>(null)

watch(() => props.src, () => { failed.value = false })

// Com render no servidor o browser ja tentou buscar a imagem antes de o
// listener existir, portanto o evento de erro perde-se. Aqui pergunta-se
// directamente ao elemento como correu.
function checkLoaded() {
  const el = photoEl.value
  if (el && el.complete && el.naturalWidth === 0) { failed.value = true }
}

onMounted(checkLoaded)

const showPhoto = computed(() => Boolean(props.src) && !failed.value)
</script>

<template>
  <img
    v-if="showPhoto"
    ref="photoEl"
    class="vehicle__photo"
    :src="src"
    :alt="alt"
    @error="failed = true"
    @load="checkLoaded"
  >

  <div v-else class="vehicle__empty">
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <rect x="3" y="5" width="18" height="14" rx="2" fill="none" stroke="currentColor" stroke-width="1.5" />
      <circle cx="8.5" cy="10" r="1.5" fill="currentColor" />
      <path d="M4 17l5-5 4 4 3-2 4 3" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round" />
    </svg>
    <p>Sem imagem de {{ alt }}</p>
  </div>
</template>

<style scoped>
.vehicle__photo {
  display: block;
  width: 100%;
  max-width: 380px;
  height: auto;
}

.vehicle__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--z-space-2);
  width: 100%;
  max-width: 380px;
  min-height: 150px;
  padding: var(--z-space-4);
  box-sizing: border-box;
  border: 1px dashed var(--z-color-border);
  border-radius: var(--z-radius);
  color: var(--z-color-text-subtle);
  text-align: center;
}

.vehicle__empty svg {
  width: 28px;
  height: 28px;
}

.vehicle__empty p {
  margin: 0;
  font-size: var(--z-font-size-sm);
}
</style>
