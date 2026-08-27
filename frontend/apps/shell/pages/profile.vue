<script setup lang="ts">
import { useAuth } from '../composables/useAuth'

const { user } = useAuth()
</script>

<template>
  <div class="profile-page">
    <div class="page-header">
      <div class="header-avatar">{{ user?.email?.substring(0, 1).toUpperCase() || 'U' }}</div>
      <div class="header-text">
        <h1 class="header-title">Definições de Conta</h1>
        <p class="header-subtitle">Gerencie sua informação de perfil</p>
      </div>
    </div>

    <div class="profile-grid">
      <div class="card">
        <h2 class="card-title">Informações da Conta</h2>
        <div class="info-list">
          <div class="info-item">
            <span class="label">Email</span>
            <span class="value">{{ user?.email || 'Não definido' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Estado da Conta</span>
            <span class="value">Ativo</span>
          </div>
          <div class="info-item">
            <span class="label">Membro desde</span>
            <span class="value">2024</span>
          </div>
        </div>
      </div>

      <div class="card">
        <h2 class="card-title">Segurança</h2>
        <div class="security-section">
          <div class="section-content">
            <h3 class="section-title">Palavra-passe</h3>
            <p class="section-description">
              Altere a sua palavra-passe regularmente para manter a sua conta segura.
            </p>
            <button class="btn">Alterar Palavra-passe</button>
          </div>

          <div class="section-divider"></div>

          <div class="section-content">
            <h3 class="section-title">Autenticação de Dois Fatores</h3>
            <p class="section-description">
              Adicione uma camada extra de segurança à sua conta.
            </p>
            <button class="btn">Ativar 2FA</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-page {
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

.profile-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 900px) {
  .profile-grid {
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

.info-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-item .label {
  font-size: 0.75rem;
  color: #999;
  text-transform: uppercase;
  font-weight: 600;
}

.info-item .value {
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
}

.security-section {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.section-content {
  padding: 1rem 0;
}

.section-content:first-child {
  padding-top: 0;
}

.section-divider {
  height: 1px;
  background: rgba(0, 0, 0, 0.08);
  margin: 0;
}

.section-title {
  margin: 0 0 0.5rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: #333;
}

.section-description {
  margin: 0 0 1rem 0;
  font-size: 0.85rem;
  color: #666;
  line-height: 1.5;
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
</style>
