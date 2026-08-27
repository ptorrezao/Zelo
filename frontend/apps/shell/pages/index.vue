<script setup lang="ts">
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import DetailGrid from '@zelo/ui/components/shadcn/DetailGrid.vue'

interface Component {
  name: string
  description: string
  route?: string
}

const components: Component[] = [
  { name: 'ZButton', description: 'Botão com variantes (primary, secondary)' },
  { name: 'ZStatusBadge', description: 'Badge de status com labels e alertas' },
  { name: 'ZInput', description: 'Campo de entrada de texto' },
  { name: 'ZSearchInput', description: 'Campo de pesquisa com ícone' },
  { name: 'ZPanel', description: 'Painel/card para agrupar conteúdo' },
  { name: 'ZSidePanel', description: 'Painel lateral (sidebar)' },
  { name: 'ZWorkspace', description: 'Container principal do layout' },
  { name: 'PageHeader (shadcn)', description: 'Cabeçalho de página com avatar e título' },
  { name: 'DetailGrid (shadcn)', description: 'Grid responsivo para cards' },
  { name: 'ZNavRail', description: 'Navegação vertical com módulos' },
  { name: 'ZAvatar', description: 'Avatar com iniciais ou imagem' },
  { name: 'ZStat', description: 'Exibição de estatística com label e valor' },
  { name: 'ZStatGroup', description: 'Grupo de estatísticas em grid' },
]
</script>

<template>
  <ZWorkspace>
    <ZPanel title="Manual de Componentes">
      <p class="lead">
        Explore os componentes disponíveis no layer partilhado (@zelo/ui).
        Clique em cada componente para ver exemplos de uso.
      </p>
    </ZPanel>

   
    <div class="components-showcase">
      
      <ZPanel title="ZButton">
        <p class="component-description">Botão com variantes</p>
        <div class="demo">
          <ZButton variant="primary">Primário</ZButton>
          <ZButton>Secundário</ZButton>
          <ZButton size="sm">Pequeno</ZButton>
          <ZButton disabled>Desativado</ZButton>
        </div>
      </ZPanel>

      <ZPanel title="ZStatusBadge">
        <p class="component-description">Badge de status com alertas</p>
        <div class="demo">
          <ZStatusBadge label="A caminho" />
          <ZStatusBadge label="Em espera" alert />
          <ZStatusBadge label="Concluído" />
        </div>
      </ZPanel>

      <ZPanel title="ZInput">
        <p class="component-description">Campo de entrada de texto</p>
        <ZInput placeholder="Campo de texto" />
      </ZPanel>

      <ZPanel title="ZSearchInput">
        <p class="component-description">Campo de pesquisa</p>
        <ZSearchInput placeholder="Procurar..." />
      </ZPanel>

      <ZPanel title="ZAvatar">
        <p class="component-description">Avatar com iniciais</p>
        <div class="demo">
          <ZAvatar name="PT" size="md" />
          <ZAvatar name="JS" size="md" />
          <ZAvatar name="MS" size="md" />
        </div>
      </ZPanel>

      <ZPanel title="ZStat">
        <p class="component-description">Exibição de estatística</p>
        <ZStat label="Total de veículos" value="3" />
        <div style="margin-top: var(--z-space-3)">
          <ZStat label="Quilómetros" value="850 km" />
        </div>
      </ZPanel>
    </div>

    <ZPanel title="PageHeader (shadcn)">
      <p class="component-description">Cabeçalho de página com avatar e título</p>
      <PageHeader
        title="Pedro Torrezão"
        subtitle="ID: 236-542-010"
        avatar-name="PT"
      />
    </ZPanel>

    <ZPanel title="DetailGrid (shadcn)">
      <p class="component-description">Grid responsivo para cards</p>
      <DetailGrid>
        <ZPanel title="Card 1">
          <p>Conteúdo do primeiro card</p>
        </ZPanel>
        <ZPanel title="Card 2">
          <p>Conteúdo do segundo card</p>
        </ZPanel>
        <ZPanel title="Card 3">
          <p>Conteúdo do terceiro card</p>
        </ZPanel>
      </DetailGrid>
    </ZPanel>
  </ZWorkspace>
</template>

<style scoped>
.lead {
  margin: 0;
  max-width: 60ch;
  color: var(--z-color-text-muted);
  line-height: 1.6;
}

.components-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--z-space-4);
  margin-bottom: var(--z-space-6);
}

.component-card {
  padding: var(--z-space-4);
  border: 1px solid var(--z-color-border);
  border-radius: var(--z-radius-md);
  background: var(--z-color-background-secondary);
  transition: all 0.2s ease;
}

.component-card:hover {
  border-color: var(--z-color-primary);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.component-header {
  margin-bottom: var(--z-space-2);
}

.component-name {
  margin: 0;
  font-size: var(--z-font-size-base);
  font-weight: 600;
  color: var(--z-color-text);
  font-family: 'Courier New', monospace;
}

.component-description {
  margin: var(--z-space-2) 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
  line-height: 1.5;
}

.component-link {
  display: inline-block;
  margin-top: var(--z-space-2);
  padding: var(--z-space-2) var(--z-space-3);
  font-size: var(--z-font-size-sm);
  color: var(--z-color-primary);
  text-decoration: none;
  border: 1px solid var(--z-color-primary);
  border-radius: var(--z-radius-sm);
  transition: all 0.2s ease;
}

.component-link:hover {
  background: var(--z-color-primary);
  color: white;
}

.demo-section {
  margin-bottom: var(--z-space-6);
}

.demo-section h4 {
  margin: 0 0 var(--z-space-3);
  font-size: var(--z-font-size-sm);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--z-color-text-muted);
}

.demo {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--z-space-3);
}

.components-showcase {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: var(--z-space-4);
  margin-bottom: var(--z-space-6);
}
</style>
