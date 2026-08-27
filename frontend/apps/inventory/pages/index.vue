<script setup lang="ts">
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import Input from '@zelo/ui/components/ui/Input.vue'

const title = 'Inventário'
const subtitle = 'Gerencie seus artigos'
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-center gap-4 rounded-lg border border-border bg-card p-6 shadow-sm">
      <Avatar name="INV" class="h-12 w-12 text-base" />
      <div>
        <h1 class="text-xl font-semibold">{{ title }}</h1>
        <p class="text-sm text-muted-foreground">{{ subtitle }}</p>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Artigos</CardTitle>
        </CardHeader>
        <CardContent>
          <p class="text-sm leading-relaxed text-muted-foreground">
            Nenhum artigo registado. Comece por adicionar um novo artigo.
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Adicionar Artigo</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="mb-6 flex flex-col gap-4">
            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">Designação</label>
              <Input placeholder="Nome do artigo" />
            </div>
            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">Localização</label>
              <Input placeholder="Onde está guardado" />
            </div>
          </div>
          <Button>Guardar</Button>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
