<script setup lang="ts">
import { useAuth } from '../composables/useAuth'
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'

const { user } = useAuth()
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-center gap-4 rounded-lg border border-border bg-card p-6 shadow-sm">
      <Avatar :name="user?.email?.substring(0, 1).toUpperCase() || 'U'" class="h-12 w-12 text-base" />
      <div>
        <h1 class="text-xl font-semibold">Definições de Conta</h1>
        <p class="text-sm text-muted-foreground">Gerencie sua informação de perfil</p>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Informações da Conta</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-6">
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Email</span>
            <span class="text-sm font-medium">{{ user?.email || 'Não definido' }}</span>
          </div>
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Estado da Conta</span>
            <span class="text-sm font-medium">Ativo</span>
          </div>
          <div class="flex flex-col gap-1">
            <span class="text-xs font-semibold uppercase text-muted-foreground">Membro desde</span>
            <span class="text-sm font-medium">2024</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Segurança</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col divide-y divide-border">
          <div class="pb-4">
            <h3 class="mb-2 text-sm font-semibold">Palavra-passe</h3>
            <p class="mb-4 text-sm leading-relaxed text-muted-foreground">
              Altere a sua palavra-passe regularmente para manter a sua conta segura.
            </p>
            <Button variant="outline" size="sm">Alterar Palavra-passe</Button>
          </div>

          <div class="pt-4">
            <h3 class="mb-2 text-sm font-semibold">Autenticação de Dois Fatores</h3>
            <p class="mb-4 text-sm leading-relaxed text-muted-foreground">
              Adicione uma camada extra de segurança à sua conta.
            </p>
            <Button variant="outline" size="sm">Ativar 2FA</Button>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
