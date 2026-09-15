<script setup lang="ts">
import { computed, ref } from 'vue'
import { Trash2 } from '@lucide/vue'
import Card from '@zelo/ui/components/ui/Card.vue'
import CardHeader from '@zelo/ui/components/ui/CardHeader.vue'
import CardTitle from '@zelo/ui/components/ui/CardTitle.vue'
import CardDescription from '@zelo/ui/components/ui/CardDescription.vue'
import CardContent from '@zelo/ui/components/ui/CardContent.vue'
import Button from '@zelo/ui/components/ui/Button.vue'
import Select from '@zelo/ui/components/ui/Select.vue'
import { useVehicles } from '../composables/useVehicles'
import type { VehicleDocument } from '../types/vehicle'

const { selected, fullName, addDocument, deleteDocument } = useVehicles()

const documentsByCategory = computed(() => {
  const groups = new Map<string, typeof selected.value.documents>()
  for (const doc of selected.value?.documents ?? []) {
    if (!groups.has(doc.category)) groups.set(doc.category, [])
    groups.get(doc.category)!.push(doc)
  }
  return Array.from(groups.entries()).map(([category, documents]) => ({ category, documents }))
})

const typeIcon: Record<string, string> = {
  pdf: '📄',
  imagem: '🖼️',
}

// Formulario de upload minimo: categoria + ficheiro, data fica sempre hoje
// (nao vale a pena um DatePicker so para isto - quem quiser outra data,
// edita o documento depois quando essa funcionalidade existir).
const CATEGORY_OPTIONS: { value: VehicleDocument['category'], label: string }[] = [
  { value: 'Seguro', label: 'Seguro' },
  { value: 'Manutenção', label: 'Manutenção' },
  { value: 'Inspeção', label: 'Inspeção' },
  { value: 'Registo', label: 'Registo' },
  { value: 'Fatura', label: 'Fatura' },
]

const isFormOpen = ref(false)
const category = ref<VehicleDocument['category']>('Fatura')
const file = ref<File | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)
const isUploading = ref(false)
const error = ref('')
const deletingId = ref('')

function handleFileChange(event: Event) {
  file.value = (event.target as HTMLInputElement).files?.[0] ?? null
}

function openForm() {
  isFormOpen.value = true
  error.value = ''
}

function closeForm() {
  isFormOpen.value = false
  category.value = 'Fatura'
  file.value = null
  if (fileInput.value) fileInput.value.value = ''
  error.value = ''
}

async function handleUpload() {
  if (!selected.value) return
  if (!file.value) {
    error.value = 'Escolha um ficheiro.'
    return
  }

  error.value = ''
  isUploading.value = true
  try {
    await addDocument(selected.value.id, file.value, category.value, new Date().toISOString().slice(0, 10))
    closeForm()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Não foi possível anexar o documento.'
  } finally {
    isUploading.value = false
  }
}

async function handleDelete(documentId: string) {
  if (!selected.value) return
  if (!confirm('Eliminar este documento?')) return

  deletingId.value = documentId
  try {
    await deleteDocument(selected.value.id, documentId)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Não foi possível eliminar o documento.'
  } finally {
    deletingId.value = ''
  }
}
</script>

<template>
  <div v-if="selected" class="mx-auto flex max-w-3xl flex-col gap-6">
    <div>
      <NuxtLink to="/" class="text-sm text-muted-foreground transition-colors hover:text-foreground">
        ← {{ fullName(selected) }}
      </NuxtLink>
    </div>

    <Card>
      <CardHeader class="flex flex-row items-start justify-between gap-4">
        <div>
          <CardTitle>Documentos</CardTitle>
          <CardDescription>{{ fullName(selected) }} · {{ selected?.plate }}</CardDescription>
        </div>
        <Button v-if="!isFormOpen" size="sm" @click="openForm">Adicionar documento</Button>
      </CardHeader>
      <CardContent class="flex flex-col gap-6">
        <form v-if="isFormOpen" class="flex flex-col gap-4 rounded-md border border-border p-4" @submit.prevent="handleUpload">
          <div
            v-if="error"
            class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          >
            {{ error }}
          </div>

          <div class="flex flex-col gap-2">
            <label class="text-sm font-medium">Categoria</label>
            <Select v-model="category" :options="CATEGORY_OPTIONS" />
          </div>

          <div class="flex flex-col gap-2">
            <label for="document-file" class="text-sm font-medium">Ficheiro</label>
            <input
              id="document-file"
              ref="fileInput"
              type="file"
              accept="application/pdf,image/*"
              class="text-sm file:mr-3 file:rounded-md file:border-0 file:bg-secondary file:px-3 file:py-2 file:text-sm file:font-medium"
              @change="handleFileChange"
            >
          </div>

          <div class="flex justify-end gap-2">
            <Button type="button" variant="outline" size="sm" :disabled="isUploading" @click="closeForm">Cancelar</Button>
            <Button type="submit" size="sm" :disabled="isUploading">{{ isUploading ? 'A enviar...' : 'Anexar' }}</Button>
          </div>
        </form>

        <div
          v-if="error && !isFormOpen"
          class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
        >
          {{ error }}
        </div>

        <p v-if="!selected?.documents?.length" class="text-sm text-muted-foreground">
          Nenhum documento disponível para este veículo.
        </p>

        <div v-for="group in documentsByCategory" :key="group.category" class="flex flex-col gap-2">
          <h3 class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">{{ group.category }}</h3>
          <div class="flex flex-col divide-y divide-border rounded-md border border-border">
            <div
              v-for="doc in group.documents"
              :key="doc.id"
              class="flex items-center gap-3 p-3 transition-colors hover:bg-accent"
            >
              <a :href="doc.downloadUrl" target="_blank" rel="noopener" class="flex min-w-0 flex-1 items-center gap-3">
                <span class="text-xl">{{ typeIcon[doc.type] || '📄' }}</span>
                <div class="flex min-w-0 flex-1 flex-col">
                  <span class="truncate text-sm font-medium">{{ doc.name }}</span>
                  <span class="text-xs text-muted-foreground">{{ doc.date }} · {{ doc.size }}</span>
                </div>
              </a>
              <Button
                variant="ghost"
                size="icon"
                :disabled="deletingId === doc.id"
                aria-label="Eliminar documento"
                @click="handleDelete(doc.id)"
              >
                <Trash2 class="h-4 w-4 text-muted-foreground" />
              </Button>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
