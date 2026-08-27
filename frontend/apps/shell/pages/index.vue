<script setup lang="ts">
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'

const stats = [
  { label: 'Total de veículos', value: '3' },
  { label: 'Quilómetros este mês', value: '2.145 km' },
  { label: 'Custos manutenção', value: '€ 445.00' },
  { label: 'Próxima inspeção', value: '15/06/2027' },
]
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-center gap-4 rounded-lg border border-border bg-card p-6 shadow-sm">
      <Avatar name="PT" class="h-12 w-12 text-base" />
      <div>
        <h1 class="text-xl font-semibold">Bem-vindo ao Zelo</h1>
        <p class="text-sm text-muted-foreground">Seu gerenciador de frota pessoal</p>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Visão Geral</CardTitle>
        </CardHeader>
        <CardContent>
          <p class="mb-4 text-sm leading-relaxed text-muted-foreground">
            Zelo é uma aplicação para gerenciar sua frota pessoal de veículos.
            Acompanhe manutenção, quilometragem, custos e muito mais.
          </p>
          <div class="flex gap-3">
            <Button>Ver Veículos</Button>
            <Button variant="outline">Novo Veículo</Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Estatísticas</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-2">
          <div
            v-for="stat in stats"
            :key="stat.label"
            class="flex items-center justify-between rounded-md bg-muted px-3 py-2"
          >
            <span class="text-sm text-muted-foreground">{{ stat.label }}</span>
            <span class="text-sm font-semibold">{{ stat.value }}</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Aplicações</CardTitle>
        </CardHeader>
        <CardContent class="grid grid-cols-2 gap-4">
          <div class="cursor-pointer rounded-md bg-muted p-4 text-center transition-colors hover:bg-accent">
            <div class="mb-2 text-2xl">◆</div>
            <h3 class="text-sm font-semibold">Auto</h3>
            <p class="text-xs text-muted-foreground">Gerencie seus veículos</p>
          </div>
          <div class="cursor-pointer rounded-md bg-muted p-4 text-center transition-colors hover:bg-accent">
            <div class="mb-2 text-2xl">≡</div>
            <h3 class="text-sm font-semibold">Inventário</h3>
            <p class="text-xs text-muted-foreground">Organize seus artigos</p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Recursos</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-3">
          <a href="#" class="flex items-start gap-3 rounded-md bg-muted p-3 transition-colors hover:bg-accent">
            <span class="text-xl">📖</span>
            <div>
              <h4 class="text-sm font-semibold">Documentação</h4>
              <p class="text-xs text-muted-foreground">Saiba mais sobre as funcionalidades</p>
            </div>
          </a>
          <a href="#" class="flex items-start gap-3 rounded-md bg-muted p-3 transition-colors hover:bg-accent">
            <span class="text-xl">⚙️</span>
            <div>
              <h4 class="text-sm font-semibold">Definições</h4>
              <p class="text-xs text-muted-foreground">Configure a sua conta</p>
            </div>
          </a>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
