<script setup lang="ts">
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import Container from '@zelo/ui/components/shadcn/Container.vue'
import Panel from '@zelo/ui/components/shadcn/Panel.vue'
import SButton from '@zelo/ui/components/shadcn/Button.vue'

const stats = [
  { label: 'Total de veículos', value: '3' },
  { label: 'Quilómetros este mês', value: '2.145 km' },
  { label: 'Custos manutenção', value: '€ 445.00' },
  { label: 'Próxima inspeção', value: '15/06/2027' },
]
</script>

<template>
  <Container>
    <PageHeader
      title="Bem-vindo ao Zelo"
      subtitle="Seu gerenciador de frota pessoal"
      avatar-name="PT"
    />

    <div class="home-grid">
      <Panel title="Visão Geral">
        <div class="overview-content">
          <p class="intro-text">
            Zelo é uma aplicação para gerenciar sua frota pessoal de veículos.
            Acompanhe manutenção, quilometragem, custos e muito mais.
          </p>
          <div class="cta-buttons">
            <SButton variant="default">Ver Veículos</SButton>
            <SButton variant="default">Novo Veículo</SButton>
          </div>
        </div>
      </Panel>

      <Panel title="Estatísticas">
        <div class="stats-list">
          <div v-for="stat in stats" :key="stat.label" class="stat-item">
            <span class="stat-label">{{ stat.label }}</span>
            <span class="stat-value">{{ stat.value }}</span>
          </div>
        </div>
      </Panel>

      <Panel title="Aplicações">
        <div class="apps-grid">
          <div class="app-card">
            <div class="app-icon">◆</div>
            <h3 class="app-name">Auto</h3>
            <p class="app-description">Gerencie seus veículos</p>
          </div>
          <div class="app-card">
            <div class="app-icon">≡</div>
            <h3 class="app-name">Inventário</h3>
            <p class="app-description">Organize seus artigos</p>
          </div>
        </div>
      </Panel>

      <Panel title="Recursos">
        <div class="resources-list">
          <a href="#" class="resource-link">
            <span class="resource-icon">📖</span>
            <div class="resource-content">
              <h4 class="resource-title">Documentação</h4>
              <p class="resource-description">Saiba mais sobre as funcionalidades</p>
            </div>
          </a>
          <a href="#" class="resource-link">
            <span class="resource-icon">⚙️</span>
            <div class="resource-content">
              <h4 class="resource-title">Definições</h4>
              <p class="resource-description">Configure a sua conta</p>
            </div>
          </a>
        </div>
      </Panel>
    </div>
  </Container>
</template>

<style scoped>
.home-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 900px) {
  .home-grid {
    grid-template-columns: 1fr;
  }
}

.intro-text {
  margin: 0 0 1.5rem 0;
  color: #666;
  line-height: 1.6;
  font-size: 0.95rem;
}

.cta-buttons {
  display: flex;
  gap: 0.75rem;
}

.stats-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: #f9f9f9;
  border-radius: 0.5rem;
}

.stat-label {
  font-size: 0.85rem;
  color: #666;
}

.stat-value {
  font-size: 1.1rem;
  font-weight: 600;
  color: #333;
}

.apps-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.app-card {
  padding: 1rem;
  background: #f9f9f9;
  border-radius: 0.5rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.app-card:hover {
  background: #f0f0f0;
  transform: translateY(-2px);
}

.app-icon {
  font-size: 2rem;
  margin-bottom: 0.5rem;
}

.app-name {
  margin: 0.5rem 0 0.25rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: #333;
}

.app-description {
  margin: 0;
  font-size: 0.75rem;
  color: #999;
}

.resources-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.resource-link {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1rem;
  background: #f9f9f9;
  border-radius: 0.5rem;
  text-decoration: none;
  color: inherit;
  transition: all 0.2s ease;
}

.resource-link:hover {
  background: #f0f0f0;
}

.resource-icon {
  font-size: 1.5rem;
  flex: none;
}

.resource-title {
  margin: 0 0 0.25rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: #333;
}

.resource-description {
  margin: 0;
  font-size: 0.8rem;
  color: #999;
}

.overview-content {
  display: flex;
  flex-direction: column;
}
</style>
