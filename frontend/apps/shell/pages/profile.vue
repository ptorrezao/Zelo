<script setup lang="ts">
import { useAuth } from '../composables/useAuth'
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import Container from '@zelo/ui/components/shadcn/Container.vue'
import Panel from '@zelo/ui/components/shadcn/Panel.vue'
import SButton from '@zelo/ui/components/shadcn/Button.vue'

const { user } = useAuth()
</script>

<template>
  <Container>
    <PageHeader
      title="Definições de Conta"
      subtitle="Gerencie sua informação de perfil"
      :avatar-name="user?.email?.substring(0, 1).toUpperCase() || 'U'"
    />

    <div class="profile-grid">
      <Panel title="Informações da Conta">
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
      </Panel>

      <Panel title="Segurança">
        <div class="security-section">
          <div class="section-content">
            <h3 class="section-title">Palavra-passe</h3>
            <p class="section-description">
              Altere a sua palavra-passe regularmente para manter a sua conta segura.
            </p>
            <SButton variant="default" size="sm">Alterar Palavra-passe</SButton>
          </div>

          <div class="section-divider"></div>

          <div class="section-content">
            <h3 class="section-title">Autenticação de Dois Fatores</h3>
            <p class="section-description">
              Adicione uma camada extra de segurança à sua conta.
            </p>
            <SButton variant="default" size="sm">Ativar 2FA</SButton>
          </div>
        </div>
      </Panel>
    </div>
  </Container>
</template>

<style scoped>
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
</style>
