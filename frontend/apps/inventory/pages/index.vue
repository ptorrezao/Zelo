<script setup lang="ts">
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import Container from '@zelo/ui/components/shadcn/Container.vue'
import Panel from '@zelo/ui/components/shadcn/Panel.vue'
import SButton from '@zelo/ui/components/shadcn/Button.vue'
import SInput from '@zelo/ui/components/shadcn/Input.vue'

const title = 'Inventário'
const subtitle = 'Gerencie seus artigos'
</script>

<template>
  <Container>
    <PageHeader :title="title" :subtitle="subtitle" avatar-name="INV" />

    <div class="inventory-grid">
      <Panel title="Artigos">
        <p class="empty-state">
          Nenhum artigo registado. Comece por adicionar um novo artigo.
        </p>
      </Panel>

      <Panel title="Adicionar Artigo">
        <div class="form-fields">
          <div class="form-group">
            <label class="form-label">Designação</label>
            <SInput placeholder="Nome do artigo" />
          </div>
          <div class="form-group">
            <label class="form-label">Localização</label>
            <SInput placeholder="Onde está guardado" />
          </div>
        </div>
        <div class="form-actions">
          <SButton variant="default">Guardar</SButton>
        </div>
      </Panel>
    </div>
  </Container>
</template>

<style scoped>
.inventory-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 900px) {
  .inventory-grid {
    grid-template-columns: 1fr;
  }
}

.empty-state {
  margin: 0;
  font-size: 0.9rem;
  color: #666;
  line-height: 1.6;
}

.form-fields {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-size: 0.85rem;
  font-weight: 500;
  color: #333;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
}
</style>
