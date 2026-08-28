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
const { login } = useAuth()

const email = ref('')
const password = ref('')
const isLoading = ref(false)
const error = ref('')

async function handleLogin() {
  if (!email.value || !password.value) {
    error.value = 'Preencha todos os campos'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    await login(email.value, password.value)
    router.push('/')
  } catch {
    error.value = 'Email ou palavra-passe inválidos'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="grid min-h-screen grid-cols-1 lg:grid-cols-2">
    <!-- Formulário -->
    <div class="flex items-center justify-center bg-background p-6 sm:p-10">
      <div class="w-full max-w-sm">
        <div class="mb-8 flex flex-col items-center gap-4 text-center">
          <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-primary text-lg font-bold text-primary-foreground">
            Z
          </div>
          <div>
            <h1 class="text-2xl font-semibold">Bem-vindo de volta</h1>
            <p class="mt-1 text-sm text-muted-foreground">Inicie sessão na sua conta Zelo</p>
          </div>
        </div>

        <form class="flex flex-col gap-4" @submit.prevent="handleLogin">
          <div
            v-if="error"
            class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          >
            {{ error }}
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
            <div class="flex items-center justify-between">
              <label for="password" class="text-sm font-medium">Palavra-passe</label>
              <a href="/forgot-password" class="text-sm text-muted-foreground transition-colors hover:text-foreground">
                Esqueceu-se?
              </a>
            </div>
            <Input
              id="password"
              v-model="password"
              type="password"
              placeholder="••••••••"
              required
              autocomplete="current-password"
            />
          </div>

          <Button type="submit" :disabled="isLoading" class="mt-2 w-full">
            {{ isLoading ? 'A iniciar sessão...' : 'Iniciar sessão' }}
          </Button>
        </form>

        <p class="mt-6 text-center text-sm text-muted-foreground">
          Ainda não tem conta?
          <a href="/signup" class="font-medium text-foreground hover:underline">Criar conta</a>
        </p>
      </div>
    </div>

    <AuthBrandPanel />
  </div>
</template>
