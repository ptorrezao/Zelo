<script setup lang="ts">
export interface Segment {
  label: string
  value: number
  /** Slot da paleta categorica, 1 a 4. A ordem e fixa: ver tokens.css. */
  series: 1 | 2 | 3 | 4
}

const props = defineProps<{ segments: Segment[] }>()

const total = computed(() => props.segments.reduce((sum, s) => sum + s.value, 0))

const parts = computed(() =>
  props.segments.map((segment) => {
    const share = total.value === 0 ? 0 : (segment.value / total.value) * 100
    return {
      ...segment,
      share,
      percent: `${share.toFixed(1)}%`,
      // Abaixo desta largura o rotulo nao cabe com folga dentro do segmento;
      // nesse caso fica so na tabela, nunca cortado.
      showLabel: share >= 12,
    }
  }),
)
</script>

<template>
  <div class="z-segbar">
    <div class="z-segbar__labels">
      <span
        v-for="part in parts"
        :key="part.label"
        class="z-segbar__label"
        :style="{ flexGrow: part.share }"
      >{{ part.label }}</span>
    </div>

    <div class="z-segbar__track">
      <div
        v-for="part in parts"
        :key="part.label"
        class="z-segbar__segment"
        :style="{
          flexGrow: part.share,
          background: `var(--z-series-${part.series})`,
          color: `var(--z-series-${part.series}-ink)`,
        }"
      >
        <span v-if="part.showLabel">{{ part.percent }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.z-segbar__labels {
  display: flex;
  gap: 2px;
  margin-bottom: var(--z-space-2);
}

.z-segbar__label {
  flex-basis: 0;
  min-width: 0;
  font-size: var(--z-font-size-xs);
  color: var(--z-color-text-muted);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.z-segbar__track {
  /* O gap de 2px na cor da superficie e o que separa os segmentos;
     nenhum segmento leva contorno. */
  display: flex;
  gap: 2px;
  overflow: hidden;
  border-radius: var(--z-radius);
}

.z-segbar__segment {
  display: flex;
  align-items: center;
  flex-basis: 0;
  min-width: 0;
  height: 34px;
  padding: 0 var(--z-space-3);
  font-size: var(--z-font-size-sm);
  font-weight: 600;
  white-space: nowrap;
}
</style>
