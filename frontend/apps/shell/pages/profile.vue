<script setup lang="ts">
import { ref } from 'vue'
import { useAuth } from '../composables/useAuth'
import { useHousehold } from '../composables/useHousehold'
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import Input from '@zelo/ui/components/ui/Input.vue'

const { user } = useAuth()
const { households, rename, create } = useHousehold()

// Nomes em edição, indexados por household id - só entra em modo de
// edição quando o utilizador clica em "Mudar nome" naquele household.
const editingId = ref<string | null>(null)
const editingName = ref('')
const isSaving = ref(false)
const errorMessage = ref('')

function startEditing(householdId: string, currentName: string) {
  editingId.value = householdId
  editingName.value = currentName
  errorMessage.value = ''
}

async function saveEditing() {
  if (!editingId.value) return
  isSaving.value = true
  errorMessage.value = ''
  try {
    await rename(editingId.value, editingName.value)
    editingId.value = null
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Não foi possível mudar o nome.'
  } finally {
    isSaving.value = false
  }
}

const isAddingHousehold = ref(false)
const newHouseholdName = ref('')

async function addHousehold() {
  if (!newHouseholdName.value.trim()) return
  isSaving.value = true
  errorMessage.value = ''
  try {
    await create(newHouseholdName.value)
    newHouseholdName.value = ''
    isAddingHousehold.value = false
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Não foi possível criar o household.'
  } finally {
    isSaving.value = false
  }
}
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

      <Card>
        <CardHeader>
          <CardTitle>Households</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-4">
          <p v-if="errorMessage" class="rounded-md border border-destructive/30 bg-destructive/10 p-2 text-xs text-destructive">
            {{ errorMessage }}
          </p>

          <div v-for="household in households" :key="household.id" class="flex flex-col gap-2 border-b border-border pb-4 last:border-0 last:pb-0">
            <template v-if="editingId === household.id">
              <Input v-model="editingName" placeholder="Nome do household" />
              <div class="flex gap-2">
                <Button size="sm" :disabled="isSaving" @click="saveEditing">Guardar</Button>
                <Button size="sm" variant="outline" :disabled="isSaving" @click="editingId = null">Cancelar</Button>
              </div>
            </template>
            <template v-else>
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm font-medium">{{ household.name }}</p>
                  <p class="text-xs text-muted-foreground">{{ household.role === 'Owner' ? 'Dono' : 'Membro' }}</p>
                </div>
                <Button v-if="household.role === 'Owner'" size="sm" variant="outline" @click="startEditing(household.id, household.name)">
                  Mudar nome
                </Button>
              </div>
            </template>
          </div>

          <template v-if="isAddingHousehold">
            <Input v-model="newHouseholdName" placeholder="Nome do novo household" />
            <div class="flex gap-2">
              <Button size="sm" :disabled="isSaving" @click="addHousehold">Criar</Button>
              <Button size="sm" variant="outline" :disabled="isSaving" @click="isAddingHousehold = false">Cancelar</Button>
            </div>
          </template>
          <Button v-else size="sm" variant="outline" @click="isAddingHousehold = true">+ Adicionar household</Button>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
