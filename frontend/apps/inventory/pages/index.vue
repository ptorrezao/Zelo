<script setup lang="ts">
const title = 'Inventário'
const subtitle = 'Gerencie seus artigos'
</script>

<template>
  <div class="inventory-page">
    <div class="page-header">
      <div class="header-avatar">INV</div>
      <div class="header-text">
        <h1 class="header-title">{{ title }}</h1>
        <p class="header-subtitle">{{ subtitle }}</p>
      </div>
    </div>

    <div class="inventory-grid">
      <div class="card">
        <h2 class="card-title">Artigos</h2>
        <p class="empty-state">
          Nenhum artigo registado. Comece por adicionar um novo artigo.
        </p>
      </div>

      <div class="card">
        <h2 class="card-title">Adicionar Artigo</h2>
        <div class="form-fields">
          <div class="form-group">
            <label class="form-label">Designação</label>
            <input type="text" class="form-input" placeholder="Nome do artigo" />
          </div>
          <div class="form-group">
            <label class="form-label">Localização</label>
            <input type="text" class="form-input" placeholder="Onde está guardado" />
          </div>
        </div>
        <div class="form-actions">
          <button class="btn btn-primary">Guardar</button>
        </div>
      </div>
    </div>
  </div>
</template>
