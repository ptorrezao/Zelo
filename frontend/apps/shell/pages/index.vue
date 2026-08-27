<template>
  <ZWorkspace>
    <ZPanel title="Zelo">
      <p class="lead">
        Gestão de ativos pessoais e das obrigações que trazem consigo.
        Escolha um módulo no rail à esquerda.
      </p>
    </ZPanel>

    <ZPanel title="Componentes do layer">
      <p class="lead">
        Nenhum destes componentes foi importado nesta app: vêm todos do
        layer partilhado, tal como o rail de navegação.
      </p>
      <div class="demo">
        <ZButton variant="primary">Primário</ZButton>
        <ZButton>Secundário</ZButton>
        <ZStatusBadge label="A caminho" />
        <ZStatusBadge label="Em espera" alert />
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

.demo {
  display: flex;
  align-items: center;
  gap: var(--z-space-4);
  margin-top: var(--z-space-4);
}
</style>
