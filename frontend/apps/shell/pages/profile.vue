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
