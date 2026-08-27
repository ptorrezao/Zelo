<script setup lang="ts">
const stats = [
  { label: 'Total de veículos', value: '3' },
  { label: 'Quilómetros este mês', value: '2.145 km' },
  { label: 'Custos manutenção', value: '€ 445.00' },
  { label: 'Próxima inspeção', value: '15/06/2027' },
]
</script>

<template>
  <div class="home-page">
    <div class="page-header">
      <div class="header-avatar">PT</div>
      <div class="header-text">
        <h1 class="header-title">Bem-vindo ao Zelo</h1>
        <p class="header-subtitle">Seu gerenciador de frota pessoal</p>
      </div>
    </div>

    <div class="home-grid">
      <div class="card">
        <h2 class="card-title">Visão Geral</h2>
        <p class="card-text">
          Zelo é uma aplicação para gerenciar sua frota pessoal de veículos.
          Acompanhe manutenção, quilometragem, custos e muito mais.
        </p>
        <div class="card-buttons">
          <button class="btn btn-primary">Ver Veículos</button>
          <button class="btn">Novo Veículo</button>
        </div>
      </div>

      <div class="card">
        <h2 class="card-title">Estatísticas</h2>
        <div class="stats-list">
          <div v-for="stat in stats" :key="stat.label" class="stat-item">
            <span class="stat-label">{{ stat.label }}</span>
            <span class="stat-value">{{ stat.value }}</span>
          </div>
        </div>
      </div>

      <div class="card">
        <h2 class="card-title">Aplicações</h2>
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
      </div>

      <div class="card">
        <h2 class="card-title">Recursos</h2>
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
      </div>
    </div>
  </div>
</template>

<style scoped>
.home-page {
  padding: 1.5rem;
  background: #f5f5f5;
  min-height: 100vh;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 1.5rem;
}

.header-avatar {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0066cc;
  color: white;
  border-radius: 50%;
  font-weight: 600;
  font-size: 1rem;
  flex-shrink: 0;
}

.header-text {
  flex: 1;
}

.header-title {
  margin: 0 0 0.25rem 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: #333;
}

.header-subtitle {
  margin: 0;
  font-size: 0.95rem;
  color: #999;
}

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

.card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.card-title {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: #333;
  border-bottom: 1px solid #eee;
  padding-bottom: 1rem;
}

.card-text {
  margin: 0 0 1.5rem 0;
  color: #666;
  line-height: 1.6;
  font-size: 0.95rem;
}

.card-buttons {
  display: flex;
  gap: 0.75rem;
}

.btn {
  padding: 0.75rem 1rem;
  background: #f0f0f0;
  color: #333;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
}

.btn:hover {
  background: #e0e0e0;
}

.btn-primary {
  background: #0066cc;
  color: white;
  border: none;
}

.btn-primary:hover {
  background: #0052a3;
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
  border-radius: 6px;
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
  border-radius: 6px;
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
  border-radius: 6px;
  text-decoration: none;
  color: inherit;
  transition: all 0.2s ease;
}

.resource-link:hover {
  background: #f0f0f0;
}

.resource-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
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
</style>
