<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from '@zelo/ui/components/ui/Button.vue'
import Input from '@zelo/ui/components/ui/Input.vue'
import AuthBrandPanel from '../components/AuthBrandPanel.vue'
import { useAuth } from '../composables/useAuth'

definePageMeta({
  layout: 'blank',
})

const router = useRouter()
const { register } = useAuth()

const name = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const isLoading = ref(false)
const error = ref('')
const isSubmitted = ref(false)

async function handleSignup() {
  if (!name.value || !email.value || !password.value || !confirmPassword.value) {
    error.value = 'Preencha todos os campos'
    return
  }

  if (password.value !== confirmPassword.value) {
    error.value = 'As palavras-passe não coincidem'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    await register(email.value, password.value)
    // A conta fica por confirmar (RequireConfirmedEmail no backend) - so
    // depois de clicar no link enviado por email e que o login funciona.
    isSubmitted.value = true
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Não foi possível criar a conta'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="grid min-h-screen grid-cols-1 lg:grid-cols-2">
    <div class="flex items-center justify-center bg-background p-6 sm:p-10">
      <div class="w-full max-w-sm">
        <div class="mb-8 flex flex-col items-center gap-4 text-center">
          <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-primary text-lg font-bold text-primary-foreground">
            Z
          </div>
          <div>
            <h1 class="text-2xl font-semibold">Criar conta</h1>
            <p class="mt-1 text-sm text-muted-foreground">
              {{ isSubmitted ? 'Verifique o seu email' : 'Comece a usar o Zelo gratuitamente' }}
            </p>
          </div>
        </div>

        <form v-if="!isSubmitted" class="flex flex-col gap-4" @submit.prevent="handleSignup">
          <div
            v-if="error"
            class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          >
            {{ error }}
          </div>

          <div class="flex flex-col gap-2">
            <label for="name" class="text-sm font-medium">Nome</label>
            <Input id="name" v-model="name" type="text" placeholder="O seu nome" required autocomplete="name" />
          </div>

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

          <div class="flex flex-col gap-2">
            <label for="password" class="text-sm font-medium">Palavra-passe</label>
            <Input
              id="password"
              v-model="password"
              type="password"
              placeholder="••••••••"
              required
              autocomplete="new-password"
            />
          </div>

          <div class="flex flex-col gap-2">
            <label for="confirm-password" class="text-sm font-medium">Confirmar palavra-passe</label>
            <Input
              id="confirm-password"
              v-model="confirmPassword"
              type="password"
              placeholder="••••••••"
              required
              autocomplete="new-password"
            />
          </div>

          <Button type="submit" :disabled="isLoading" class="mt-2 w-full">
            {{ isLoading ? 'A criar conta...' : 'Criar conta' }}
          </Button>
        </form>

        <div v-else class="flex flex-col gap-4">
          <p class="text-sm leading-relaxed">
            Enviámos um link de confirmação para <strong>{{ email }}</strong>
          </p>
          <Button class="mt-2 w-full" @click="router.push('/login')">
            Voltar a iniciar sessão
          </Button>
        </div>

        <p v-if="!isSubmitted" class="mt-6 text-center text-sm text-muted-foreground">
          Já tem conta?
          <a href="/login" class="font-medium text-foreground hover:underline">Iniciar sessão</a>
        </p>
      </div>
    </div>

    <AuthBrandPanel />
  </div>
</template>
