<template>
  <svg class="map" viewBox="0 0 400 150" role="img" aria-label="Traçado do percurso atual">
    <rect width="400" height="150" fill="var(--z-color-surface)" />

    <g stroke="var(--z-color-bg)" stroke-width="7">
      <path d="M0 38h400M0 88h400M0 128h400" />
      <path d="M60 0v150M150 0v150M250 0v150M330 0v150" />
    </g>

    <path
      d="M60 108h90V38h100v50h80"
      fill="none"
      stroke="var(--z-color-text)"
      stroke-width="3"
      stroke-linecap="round"
      stroke-linejoin="round"
      stroke-dasharray="1 7"
    />

    <circle cx="60" cy="108" r="11" fill="var(--z-color-text)" />
    <path d="M56 108h8M61 104l4 4-4 4" stroke="var(--z-color-bg)" stroke-width="1.6" fill="none" stroke-linecap="round" stroke-linejoin="round" />

    <path
      d="M330 76c-6 0-11 5-11 11 0 8 11 20 11 20s11-12 11-20c0-6-5-11-11-11z"
      fill="var(--z-color-text)"
    />
    <circle cx="330" cy="87" r="4" fill="var(--z-color-bg)" />
  </svg>
</template>

<style scoped>
.map {
  display: block;
  width: 100%;
  height: auto;
  border-radius: var(--z-radius);
}
</style>
