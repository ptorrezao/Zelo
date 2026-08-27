<script setup lang="ts">
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import DetailGrid from '@zelo/ui/components/shadcn/DetailGrid.vue'

const title = 'Inventory'
const subtitle = 'Manage your inventory items'
</script>

<template>
  <ZWorkspace>
    <div class="inventory__wrapper">
      <ZPanel>
        <PageHeader
          :title="title"
          :subtitle="subtitle"
          avatar-name="Inventory"
        />
      </ZPanel>

      <DetailGrid>
        <ZPanel title="Items">
          <p class="inventory__empty">
            Nenhum artigo registado. Comece por adicionar um novo artigo.
          </p>
        </ZPanel>

        <ZPanel title="Add Item">
          <div class="inventory__form">
            <ZInput label="Designação" placeholder="Nome do artigo" />
            <ZInput label="Localização" placeholder="Onde está guardado" />
          </div>
          <div class="inventory__actions">
            <ZButton variant="primary">Guardar</ZButton>
          </div>
        </ZPanel>
      </DetailGrid>
    </div>
  </ZWorkspace>
</template>

<style scoped>
.inventory__wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-6);
}

.inventory__empty {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
  line-height: 1.6;
}

.inventory__form {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(220px, 100%), 1fr));
  gap: var(--z-space-4);
  margin-bottom: var(--z-space-4);
}

.inventory__actions {
  display: flex;
  gap: var(--z-space-3);
}
</style>
