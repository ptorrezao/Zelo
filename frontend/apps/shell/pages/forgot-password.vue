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
const { forgotPassword, resetPassword } = useAuth()

// 'email' -> pede o email; 'reset' -> pede o codigo + password nova
// (o email da Zelo manda um codigo, nao um link - ver SmtpEmailSender);
// 'done' -> confirmacao antes de voltar ao login.
const step = ref<'email' | 'reset' | 'done'>('email')

const email = ref('')
const resetCode = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const isLoading = ref(false)
const error = ref('')

async function handleRequestCode() {
  if (!email.value) return

  isLoading.value = true
  try {
    await forgotPassword(email.value)
    step.value = 'reset'
  } finally {
    isLoading.value = false
  }
}

async function handleResetPassword() {
  error.value = ''

  if (!resetCode.value || !newPassword.value || !confirmPassword.value) {
    error.value = 'Preencha todos os campos'
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    error.value = 'As palavras-passe não coincidem'
    return
  }

  isLoading.value = true
  try {
    await resetPassword(email.value, resetCode.value, newPassword.value)
    step.value = 'done'
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Não foi possível repor a palavra-passe.'
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
            <template v-if="step === 'email'">Indique o seu email para receber um código de recuperação</template>
            <template v-else-if="step === 'reset'">Introduza o código que enviámos e a nova palavra-passe</template>
            <template v-else>Palavra-passe reposta</template>
          </p>
        </div>

        <form v-if="step === 'email'" class="flex flex-col gap-4" @submit.prevent="handleRequestCode">
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
            {{ isLoading ? 'A enviar...' : 'Enviar código de recuperação' }}
          </Button>
        </form>

        <form v-else-if="step === 'reset'" class="flex flex-col gap-4" @submit.prevent="handleResetPassword">
          <p class="text-sm leading-relaxed">
            Enviámos um código para <strong>{{ email }}</strong>. Copie-o do email e cole-o abaixo.
          </p>

          <div
            v-if="error"
            class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          >
            {{ error }}
          </div>

          <div class="flex flex-col gap-2">
            <label for="reset-code" class="text-sm font-medium">Código</label>
            <Input id="reset-code" v-model="resetCode" type="text" placeholder="Código recebido por email" required />
          </div>

          <div class="flex flex-col gap-2">
            <label for="new-password" class="text-sm font-medium">Nova palavra-passe</label>
            <Input
              id="new-password"
              v-model="newPassword"
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
            {{ isLoading ? 'A repor...' : 'Repor palavra-passe' }}
          </Button>

          <button
            type="button"
            class="text-sm text-muted-foreground transition-colors hover:text-foreground"
            @click="handleRequestCode"
          >
            Não recebeu? Enviar novo código
          </button>
        </form>

        <div v-else class="flex flex-col gap-4">
          <p class="text-sm leading-relaxed">
            A sua palavra-passe foi reposta. Já pode iniciar sessão com a nova palavra-passe.
          </p>

          <Button class="mt-2 w-full" @click="router.push('/login')">
            Iniciar sessão
          </Button>
        </div>
      </div>
    </div>

    <AuthBrandPanel />
  </div>
</template>
