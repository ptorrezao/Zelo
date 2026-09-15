<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAuth } from '../composables/useAuth'
import { useHousehold } from '../composables/useHousehold'
import { useApiKeys } from '../composables/useApiKeys'
import { useNotifications } from '@zelo/ui/composables/useNotifications'
import { useCurrentUser } from '@zelo/ui/composables/useCurrentUser'
import Avatar from '@zelo/ui/components/ui/Avatar.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import Input from '@zelo/ui/components/ui/Input.vue'

const { user } = useAuth()
const { data: currentUser, updateName } = useCurrentUser()
const { households, rename, create, remove } = useHousehold()

const isEditingName = ref(false)
const nameInput = ref('')
const isSavingName = ref(false)
const nameErrorMessage = ref('')

function startEditingName() {
  nameInput.value = currentUser.value?.name ?? ''
  isEditingName.value = true
  nameErrorMessage.value = ''
}

async function saveName() {
  isSavingName.value = true
  nameErrorMessage.value = ''
  try {
    await updateName(nameInput.value)
    isEditingName.value = false
  } catch (err) {
    nameErrorMessage.value = err instanceof Error ? err.message : 'Não foi possível guardar o nome.'
  } finally {
    isSavingName.value = false
  }
}

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

// Confirmacao inline (sem modal na app) - clicar em "Remover" mostra
// "tem a certeza?" no lugar dos botoes normais, so para aquele household.
const removingId = ref<string | null>(null)

function startRemoving(householdId: string) {
  removingId.value = householdId
  errorMessage.value = ''
}

async function confirmRemove(householdId: string) {
  isSaving.value = true
  errorMessage.value = ''
  try {
    await remove(householdId)
    removingId.value = null
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Não foi possível remover o household.'
  } finally {
    isSaving.value = false
  }
}

// Chaves de API - credencial de longa duracao para agentes/integracoes
// (ex.: MCP do Auto), separada da sessao normal.
const { apiKeys, create: createApiKey, revoke: revokeApiKey } = useApiKeys()

const isAddingApiKey = ref(false)
const newApiKeyName = ref('')
const isSavingApiKey = ref(false)
const apiKeyErrorMessage = ref('')

// So preenchido logo depois de criar uma chave - e a unica vez que o
// valor em claro existe (o backend so guarda o hash), por isso mostra-se
// aqui uma vez e depois perde-se para sempre.
const revealedKey = ref<{ name: string, key: string } | null>(null)
const copyFeedback = ref(false)

async function addApiKey() {
  if (!newApiKeyName.value.trim()) return
  isSavingApiKey.value = true
  apiKeyErrorMessage.value = ''
  try {
    const key = await createApiKey(newApiKeyName.value)
    revealedKey.value = { name: newApiKeyName.value, key }
    newApiKeyName.value = ''
    isAddingApiKey.value = false
  } catch (err) {
    apiKeyErrorMessage.value = err instanceof Error ? err.message : 'Não foi possível criar a chave.'
  } finally {
    isSavingApiKey.value = false
  }
}

async function copyRevealedKey() {
  if (!revealedKey.value) return
  try {
    await navigator.clipboard.writeText(revealedKey.value.key)
    copyFeedback.value = true
    setTimeout(() => (copyFeedback.value = false), 2000)
  } catch {
    // Permissao de clipboard negada (browser/contexto sem HTTPS, ou
    // recusada pelo utilizador) - a chave continua visivel no ecra para
    // copiar a mao, so o botao deixa de dar feedback de sucesso.
    apiKeyErrorMessage.value = 'Não foi possível copiar automaticamente - copia o valor acima manualmente.'
  }
}

const revokingApiKeyId = ref<string | null>(null)

async function confirmRevokeApiKey(id: string) {
  isSavingApiKey.value = true
  apiKeyErrorMessage.value = ''
  try {
    await revokeApiKey(id)
    revokingApiKeyId.value = null
  } catch (err) {
    apiKeyErrorMessage.value = err instanceof Error ? err.message : 'Não foi possível revogar a chave.'
  } finally {
    isSavingApiKey.value = false
  }
}

// Preferencias de notificacao - so do household "primario" (o mesmo que
// useNotifications resolve para o sino), mesma simplificacao ja usada em
// toda a app para quem tem varios households.
const { loadPreferences, savePreferences } = useNotifications()
const daysWarning = ref(15)
const emailEnabled = ref(true)
const isLoadingPreferences = ref(true)
const isSavingPreferences = ref(false)
const preferencesSaved = ref(false)
const preferencesError = ref('')

onMounted(async () => {
  try {
    const preferences = await loadPreferences()
    if (preferences) {
      daysWarning.value = Number(preferences.daysWarning)
      emailEnabled.value = preferences.emailEnabled
    }
  } finally {
    isLoadingPreferences.value = false
  }
})

async function saveNotificationPreferences() {
  isSavingPreferences.value = true
  preferencesError.value = ''
  preferencesSaved.value = false
  try {
    await savePreferences({ daysWarning: daysWarning.value, emailEnabled: emailEnabled.value })
    preferencesSaved.value = true
    setTimeout(() => (preferencesSaved.value = false), 2000)
  } catch (err) {
    preferencesError.value = err instanceof Error ? err.message : 'Não foi possível guardar as preferências.'
  } finally {
    isSavingPreferences.value = false
  }
}

function formatDate(iso: string | null | undefined): string {
  if (!iso) return 'Nunca'
  return new Date(iso).toLocaleDateString('pt-PT', { day: '2-digit', month: '2-digit', year: 'numeric' })
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-center gap-4 rounded-lg border border-border bg-card p-6 shadow-sm">
      <Avatar :name="currentUser?.name || user?.email?.substring(0, 1).toUpperCase() || 'U'" class="h-12 w-12 text-base" />
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
            <span class="text-xs font-semibold uppercase text-muted-foreground">Nome</span>
            <p v-if="nameErrorMessage" class="rounded-md border border-destructive/30 bg-destructive/10 p-2 text-xs text-destructive">
              {{ nameErrorMessage }}
            </p>
            <template v-if="isEditingName">
              <Input v-model="nameInput" placeholder="O seu nome" />
              <div class="flex gap-2">
                <Button size="sm" :disabled="isSavingName" @click="saveName">Guardar</Button>
                <Button size="sm" variant="outline" :disabled="isSavingName" @click="isEditingName = false">Cancelar</Button>
              </div>
            </template>
            <div v-else class="flex items-center justify-between">
              <span class="text-sm font-medium">{{ currentUser?.name || 'Não definido' }}</span>
              <Button size="sm" variant="outline" @click="startEditingName">Editar</Button>
            </div>
          </div>
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
            <template v-else-if="removingId === household.id">
              <p class="text-sm">Remover "{{ household.name }}"? Os veículos, ativos e obrigações passam para o household predefinido.</p>
              <div class="flex gap-2">
                <Button size="sm" variant="outline" :disabled="isSaving" @click="confirmRemove(household.id)">Sim, remover</Button>
                <Button size="sm" variant="outline" :disabled="isSaving" @click="removingId = null">Cancelar</Button>
              </div>
            </template>
            <template v-else>
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm font-medium">
                    {{ household.name }}
                    <span v-if="household.isDefault" class="ml-1 text-xs font-normal text-muted-foreground">(predefinido)</span>
                  </p>
                  <p class="text-xs text-muted-foreground">{{ household.role === 'Owner' ? 'Dono' : 'Membro' }}</p>
                </div>
                <div v-if="household.role === 'Owner'" class="flex gap-2">
                  <Button size="sm" variant="outline" @click="startEditing(household.id, household.name)">
                    Mudar nome
                  </Button>
                  <Button v-if="!household.isDefault" size="sm" variant="outline" @click="startRemoving(household.id)">
                    Remover
                  </Button>
                </div>
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

      <Card>
        <CardHeader>
          <CardTitle>Chaves de API</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-4">
          <p class="text-sm text-muted-foreground">
            Para ligar agentes ou integrações (ex.: o servidor MCP do Auto) sem depender da sessão do browser, que expira em 1 hora.
          </p>

          <p v-if="apiKeyErrorMessage" class="rounded-md border border-destructive/30 bg-destructive/10 p-2 text-xs text-destructive">
            {{ apiKeyErrorMessage }}
          </p>

          <div v-if="revealedKey" class="flex flex-col gap-2 rounded-md border border-amber-500/40 bg-amber-500/10 p-3">
            <p class="text-xs font-semibold text-amber-700">
              Copia agora "{{ revealedKey.name }}" — não vais voltar a vê-la.
            </p>
            <code class="break-all rounded bg-background/60 p-2 text-xs">{{ revealedKey.key }}</code>
            <div class="flex gap-2">
              <Button size="sm" variant="outline" @click="copyRevealedKey">{{ copyFeedback ? 'Copiado!' : 'Copiar' }}</Button>
              <Button size="sm" variant="outline" @click="revealedKey = null">Fechar</Button>
            </div>
          </div>

          <div v-if="apiKeys.length === 0" class="text-sm text-muted-foreground">
            Ainda não tens nenhuma chave de API.
          </div>

          <div v-for="apiKey in apiKeys" :key="apiKey.id" class="flex flex-col gap-2 border-b border-border pb-4 last:border-0 last:pb-0">
            <template v-if="revokingApiKeyId === apiKey.id">
              <p class="text-sm">Revogar "{{ apiKey.name }}"? Deixa de dar acesso de imediato - não pode ser desfeito.</p>
              <div class="flex gap-2">
                <Button size="sm" variant="outline" :disabled="isSavingApiKey" @click="confirmRevokeApiKey(apiKey.id)">Sim, revogar</Button>
                <Button size="sm" variant="outline" :disabled="isSavingApiKey" @click="revokingApiKeyId = null">Cancelar</Button>
              </div>
            </template>
            <template v-else>
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm font-medium">
                    {{ apiKey.name }}
                    <span v-if="apiKey.revokedAt" class="ml-1 text-xs font-normal text-destructive">(revogada)</span>
                  </p>
                  <p class="text-xs text-muted-foreground">
                    {{ apiKey.displayPrefix }}… · criada em {{ formatDate(apiKey.createdAt) }} · última utilização: {{ formatDate(apiKey.lastUsedAt) }}
                  </p>
                </div>
                <Button
                  v-if="!apiKey.revokedAt"
                  size="sm"
                  variant="outline"
                  @click="revokingApiKeyId = apiKey.id"
                >
                  Revogar
                </Button>
              </div>
            </template>
          </div>

          <template v-if="isAddingApiKey">
            <Input v-model="newApiKeyName" placeholder="Nome da chave, ex. Claude" />
            <div class="flex gap-2">
              <Button size="sm" :disabled="isSavingApiKey" @click="addApiKey">Criar</Button>
              <Button size="sm" variant="outline" :disabled="isSavingApiKey" @click="isAddingApiKey = false">Cancelar</Button>
            </div>
          </template>
          <Button v-else size="sm" variant="outline" @click="isAddingApiKey = true">+ Nova chave de API</Button>
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Notificações</CardTitle>
        </CardHeader>
        <CardContent class="flex flex-col gap-4">
          <p class="text-sm text-muted-foreground">
            Lembretes por email de obrigações a vencer (seguro, inspeção), enviados uma vez por obrigação.
          </p>

          <p v-if="preferencesError" class="rounded-md border border-destructive/30 bg-destructive/10 p-2 text-xs text-destructive">
            {{ preferencesError }}
          </p>

          <template v-if="!isLoadingPreferences">
            <div class="flex flex-col gap-1">
              <label for="days-warning" class="text-xs font-semibold uppercase text-muted-foreground">Avisar com quantos dias de antecedência</label>
              <input id="days-warning" v-model.number="daysWarning" type="number" min="1" max="90" class="h-10 w-24 rounded-md border border-input bg-background px-3 py-2 text-sm">
            </div>

            <label class="flex items-center gap-2 text-sm">
              <input v-model="emailEnabled" type="checkbox" class="h-4 w-4">
              Receber por email
            </label>

            <div class="flex items-center gap-2">
              <Button size="sm" :disabled="isSavingPreferences" @click="saveNotificationPreferences">
                {{ isSavingPreferences ? 'A guardar...' : 'Guardar' }}
              </Button>
              <span v-if="preferencesSaved" class="text-xs text-muted-foreground">Guardado.</span>
            </div>
          </template>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
