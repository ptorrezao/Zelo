<template>
  <ZWorkspace>
    <ZPanel title="Inventário">
      <p class="lead">
        Ainda sem artigos. Este módulo partilha o rail, os painéis e os
        primitivos com os restantes.
      </p>
    </ZPanel>

    <ZPanel title="Novo artigo">
      <div class="form">
        <ZInput label="Designação" />
        <ZInput label="Localização" />
      </div>
      <div class="form__actions">
        <ZButton variant="primary">Guardar</ZButton>
      </div>
    </ZPanel>
  </ZWorkspace>
</template>

<style scoped>
.lead {
  margin: 0;
  max-width: 60ch;
  color: var(--z-color-text-muted);
}

.form {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(220px, 100%), 1fr));
  gap: var(--z-space-4);
}

.form__actions {
  margin-top: var(--z-space-4);
}
</style>
