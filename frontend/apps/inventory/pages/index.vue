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

<style scoped>
.inventory-page {
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

.card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.card-title {
  margin: 0 0 1rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #333;
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

.form-input {
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.9rem;
  font-family: inherit;
}

.form-input:focus {
  outline: none;
  border-color: #0066cc;
  box-shadow: 0 0 0 3px rgba(0, 102, 204, 0.1);
}

.form-actions {
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
</style>
