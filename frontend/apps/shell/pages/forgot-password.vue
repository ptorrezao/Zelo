<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from '@zelo/ui/components/ui/Button.vue'
import Input from '@zelo/ui/components/ui/Input.vue'
import AuthBrandPanel from '../components/AuthBrandPanel.vue'

definePageMeta({
  layout: 'blank',
})

const router = useRouter()
const email = ref('')
const isLoading = ref(false)
const isSubmitted = ref(false)

async function handleSubmit() {
  if (!email.value) return

  isLoading.value = true
  try {
    // TODO: Implementar chamada real à API de recuperação de palavra-passe
    await new Promise(resolve => setTimeout(resolve, 1000))
    isSubmitted.value = true
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="grid min-h-screen grid-cols-1 lg:grid-cols-2">
    <div class="flex items-center justify-center bg-background p-6 sm:p-10">
      <div class="w-full max-w-sm">
        <a href="/login" class="mb-6 inline-block text-sm text-muted-foreground transition-colors hover:text-foreground">
          ← Voltar a iniciar sessão
        </a>

        <div class="mb-8">
          <h1 class="text-2xl font-semibold">Recuperar palavra-passe</h1>
          <p class="mt-1 text-sm text-muted-foreground">
            {{ isSubmitted ? 'Verifique o seu email' : 'Indique o seu email para receber um link de recuperação' }}
          </p>
        </div>

        <form v-if="!isSubmitted" class="flex flex-col gap-4" @submit.prevent="handleSubmit">
          <div class="flex flex-col gap-2">
            <label for="email" class="text-sm font-medium">Email</label>
            <Input
              id="email"
              v-model="email"
              type="email"
              placeholder="seu@email.com"
              required
              autocomplete="email"
            />
          </div>

          <Button type="submit" :disabled="isLoading" class="mt-2 w-full">
            {{ isLoading ? 'A enviar...' : 'Enviar link de recuperação' }}
          </Button>
        </form>

        <div v-else class="flex flex-col gap-4">
          <p class="text-sm leading-relaxed">
            Enviámos um link de recuperação para <strong>{{ email }}</strong>
          </p>
          <p class="text-sm text-muted-foreground">O link expira em 10 minutos.</p>

          <Button class="mt-2 w-full" @click="router.push('/login')">
            Voltar a iniciar sessão
          </Button>
        </div>
      </div>
    </div>

    <AuthBrandPanel />
  </div>
</template>
