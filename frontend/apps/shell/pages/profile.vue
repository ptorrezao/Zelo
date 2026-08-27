<script setup lang="ts">
import { useAuth } from '../composables/useAuth'
import PageHeader from '@zelo/ui/components/shadcn/PageHeader.vue'
import DetailGrid from '@zelo/ui/components/shadcn/DetailGrid.vue'

const { user } = useAuth()
</script>

<template>
  <ZWorkspace>
    <div class="profile__wrapper">
      <ZPanel>
        <PageHeader
          title="Account Settings"
          subtitle="Manage your profile information"
          :avatar-name="user?.email || 'User'"
        />
      </ZPanel>

      <DetailGrid>
        <ZPanel title="Account Information">
          <ZStatGroup :columns="1">
            <ZStat label="Email Address" :value="user?.email || 'Not set'" />
            <ZStat label="Account Status" value="Active" />
            <ZStat label="Member Since" value="2024" />
          </ZStatGroup>
        </ZPanel>

        <ZPanel title="Security">
          <div class="profile__section">
            <h3 class="profile__section-title">Password</h3>
            <p class="profile__section-description">
              Change your password regularly to keep your account secure.
            </p>
            <ZButton size="sm" variant="primary">Change Password</ZButton>
          </div>

          <div class="profile__section">
            <h3 class="profile__section-title">Two-Factor Authentication</h3>
            <p class="profile__section-description">
              Add an extra layer of security to your account.
            </p>
            <ZButton size="sm">Enable 2FA</ZButton>
          </div>
        </ZPanel>
      </DetailGrid>
    </div>
  </ZWorkspace>
</template>

<style scoped>
.profile__wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-6);
}

.profile__section {
  padding-bottom: var(--z-space-4);
  border-bottom: 1px solid var(--z-color-border);
}

.profile__section:last-child {
  padding-bottom: 0;
  border-bottom: none;
}

.profile__section-title {
  margin: 0 0 var(--z-space-2);
  font-size: var(--z-font-size-sm);
  font-weight: 600;
  color: var(--z-color-text);
}

.profile__section-description {
  margin: 0 0 var(--z-space-3);
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
  line-height: 1.5;
}
</style>
