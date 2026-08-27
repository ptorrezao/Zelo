<script setup lang="ts">
export interface BarDatum {
  label: string
  value: number
  reference: number
}

const props = withDefaults(
  defineProps<{
    data: BarDatum[]
    valueLabel: string
    referenceLabel: string
    /** Converte o valor cru no texto mostrado na dica. */
    formatValue?: (value: number) => string
    max?: number
    showReference?: boolean
  }>(),
  { formatValue: (value: number) => String(value), max: undefined, showReference: true },
)

const hovered = ref<number | null>(null)

const scaleMax = computed(() => {
  if (props.max !== undefined) { return props.max }
  const peak = Math.max(...props.data.flatMap(d => [d.value, d.reference]), 0)
  // Arredonda para o par acima do pico, para o topo do grafico ficar junto
  // aos dados em vez de deixar meia caixa vazia.
  return Math.ceil(peak / 2) * 2 || 2
})

const ticks = computed(() => [scaleMax.value, scaleMax.value / 2, 0])

function height(value: number) {
  return `${(value / scaleMax.value) * 100}%`
}
</script>

<template>
  <figure class="z-bars">
    <div class="z-bars__frame">
      <div class="z-bars__plot" @mouseleave="hovered = null">
        <div
          v-for="(tick, index) in ticks"
          :key="index"
          class="z-bars__grid"
          :style="{ bottom: `${(tick / scaleMax) * 100}%` }"
        />

        <div
          v-for="(datum, index) in data"
          :key="datum.label"
          class="z-bars__col"
          @mouseenter="hovered = index"
        >
          <div class="z-bars__pair">
            <div v-if="showReference" class="z-bars__bar z-bars__bar--reference" :style="{ height: height(datum.reference) }" />
            <div class="z-bars__bar z-bars__bar--value" :style="{ height: height(datum.value) }" />
          </div>

          <div v-if="hovered === index" class="z-bars__tip" role="tooltip">
            <p class="z-bars__tip-row">
              <span>{{ datum.label }}</span>
              <strong>{{ formatValue(datum.value) }}</strong>
            </p>
            <p class="z-bars__tip-row z-bars__tip-row--muted">
              <span>{{ referenceLabel }}</span>
              <strong>{{ formatValue(datum.reference) }}</strong>
            </p>
          </div>
        </div>
      </div>

      <div class="z-bars__axis">
        <span v-for="tick in ticks" :key="tick">{{ tick }}</span>
      </div>
    </div>

    <figcaption class="z-bars__legend">
      <span class="z-bars__key">
        <span class="z-bars__swatch z-bars__swatch--value" />{{ valueLabel }}
      </span>
      <span v-if="showReference" class="z-bars__key">
        <span class="z-bars__swatch z-bars__swatch--reference" />{{ referenceLabel }}
      </span>
    </figcaption>
  </figure>
</template>

<style scoped>
.z-bars {
  margin: 0;
}

.z-bars__frame {
  display: flex;
  gap: var(--z-space-3);
}

.z-bars__plot {
  position: relative;
  display: flex;
  align-items: flex-end;
  gap: var(--z-space-3);
  flex: 1;
  height: 140px;
  border-bottom: 1px solid var(--z-chart-baseline);
}

.z-bars__grid {
  position: absolute;
  left: 0;
  right: 0;
  height: 1px;
  background: var(--z-chart-grid);
}

.z-bars__col {
  position: relative;
  display: flex;
  flex: 1;
  height: 100%;
  align-items: flex-end;
  justify-content: center;
}

.z-bars__pair {
  display: flex;
  align-items: flex-end;
  /* separacao entre barras vizinhas feita com espaco, nao com contorno */
  gap: 2px;
  height: 100%;
}

.z-bars__bar {
  width: 10px;
  /* extremo do dado arredondado, base assente na linha zero */
  border-radius: 4px 4px 0 0;
}

.z-bars__bar--value {
  background: var(--z-series-1);
}

.z-bars__bar--reference {
  background: var(--z-series-1-soft);
}

.z-bars__tip {
  position: absolute;
  bottom: calc(100% + var(--z-space-2));
  left: 50%;
  transform: translateX(-50%);
  z-index: 1;
  min-width: 150px;
  padding: var(--z-space-2) var(--z-space-3);
  border-radius: var(--z-radius);
  background: var(--z-color-rail);
  color: var(--z-color-rail-text-active);
  font-size: var(--z-font-size-xs);
  pointer-events: none;
}

.z-bars__tip-row {
  display: flex;
  justify-content: space-between;
  gap: var(--z-space-4);
  margin: 0;
  white-space: nowrap;
}

.z-bars__tip-row--muted {
  color: var(--z-color-rail-text);
}

.z-bars__axis {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 140px;
  font-size: var(--z-font-size-xs);
  font-variant-numeric: tabular-nums;
  color: var(--z-color-text-subtle);
}

.z-bars__legend {
  display: flex;
  justify-content: center;
  gap: var(--z-space-4);
  margin-top: var(--z-space-3);
  font-size: var(--z-font-size-xs);
  color: var(--z-color-text-muted);
}

.z-bars__key {
  display: flex;
  align-items: center;
  gap: var(--z-space-1);
}

.z-bars__swatch {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.z-bars__swatch--value {
  background: var(--z-series-1);
}

.z-bars__swatch--reference {
  background: var(--z-series-1-soft);
}
</style>
