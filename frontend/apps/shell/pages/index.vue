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
