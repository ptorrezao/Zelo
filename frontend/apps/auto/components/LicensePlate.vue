<script setup lang="ts">
const props = defineProps<{ value: string }>()

// Na chapa os grupos aparecem separados por espaco, nao por hifen.
const groups = computed(() => props.value.split('-').filter(Boolean))

// Estrela de raio 1.33 no topo de um circulo de raio 8, como na bandeira:
// mais perto do centro as doze sobrepunham-se. Fica aqui e nao no template
// porque um comentario dentro do <svg> parte a hidratacao.
const starPath = 'M12 2.67 12.3 3.59 13.27 3.59 12.49 4.16 12.78 5.08 12 4.51 11.22 5.08 11.51 4.16 10.73 3.59 11.7 3.59Z'
</script>

<template>
  <span class="plate" :aria-label="`Matrícula ${value}`" role="img">
    <span class="plate__eu" aria-hidden="true">
      <svg class="plate__stars" viewBox="0 0 24 24">
        <g v-for="index in 12" :key="index" :transform="`rotate(${index * 30} 12 12)`">
          <path class="plate__star" :d="starPath" />
        </g>
      </svg>
      <span class="plate__country">P</span>
    </span>

    <span class="plate__number">
      <span v-for="(group, index) in groups" :key="index" class="plate__group">{{ group }}</span>
    </span>
  </span>
</template>

<style scoped>
.plate {
  /* Tudo o resto sai desta medida, para a chapa encolher sem se deformar. */
  --plate-h: 44px;

  display: inline-flex;
  align-items: stretch;
  height: var(--plate-h);
  background: #fff;
  border: 2px solid #1a1a1a;
  border-radius: 5px;
  overflow: hidden;
  /* A chapa e um objeto fisico, nao um elemento do tema: as cores sao
     as da matricula portuguesa e nao seguem os tokens. */
  color: #1a1a1a;
  user-select: none;
}

@media (max-width: 900px) {
  .plate {
    --plate-h: 32px;
  }
}

.plate__eu {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1px;
  flex: none;
  width: calc(var(--plate-h) * 0.59);
  padding: calc(var(--plate-h) * 0.07) 0;
  box-sizing: border-box;
  background: #003399;
  color: #fff;
}

.plate__stars {
  width: calc(var(--plate-h) * 0.39);
  height: calc(var(--plate-h) * 0.39);
}

.plate__star {
  fill: #ffcc00;
}

.plate__country {
  font-size: calc(var(--plate-h) * 0.25);
  font-weight: 700;
  line-height: 1;
}

.plate__number {
  display: flex;
  align-items: center;
  gap: calc(var(--plate-h) * 0.23);
  padding: 0 calc(var(--plate-h) * 0.23);
  font-size: calc(var(--plate-h) * 0.5);
  font-weight: 700;
  letter-spacing: 0.04em;
  font-variant-numeric: tabular-nums;
}
</style>
