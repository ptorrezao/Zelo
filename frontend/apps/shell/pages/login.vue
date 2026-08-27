<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
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
    error.value = 'Please fill in all fields'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))

    // Perform login
    login(email.value, password.value)

    // Redirect to home
    router.push('/')
  } catch (err) {
    error.value = 'Invalid email or password'
  } finally {
    isLoading.value = false
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    handleLogin()
  }
}
</script>

<template>
  <div class="login__container">
    <!-- Left: Form -->
    <div class="login__form-side">
      <div class="login__form-content">
        <div class="login__header">
          <h1 class="login__title">Welcome back</h1>
          <p class="login__subtitle">Sign in to your Zelo account</p>
        </div>

        <form @submit.prevent="handleLogin" class="login__form">
          <div v-if="error" class="login__error">
            {{ error }}
          </div>

          <div class="login__field">
            <label for="email" class="login__label">Email</label>
            <ZInput
              id="email"
              v-model="email"
              type="email"
              placeholder="seu@email.com"
              required
              @keydown="handleKeydown"
            />
          </div>

          <div class="login__field">
            <div class="login__label-row">
              <label for="password" class="login__label">Password</label>
              <a href="/forgot-password" class="login__forgot">Forgot?</a>
            </div>
            <ZInput
              id="password"
              v-model="password"
              type="password"
              placeholder="••••••••"
              required
              @keydown="handleKeydown"
            />
          </div>

          <ZButton
            type="submit"
            variant="primary"
            :disabled="isLoading"
            class="login__button"
          >
            {{ isLoading ? 'Signing in...' : 'Sign in' }}
          </ZButton>
        </form>

        <p class="login__signup">
          No account?
          <a href="#signup" class="login__link">Start free trial</a>
        </p>
      </div>
    </div>

    <!-- Right: Branding -->
    <div class="login__brand-side">
      <div class="login__brand-content">
        <div class="login__brand-logo">Zelo</div>
        <h2 class="login__brand-title">Where teams ship together</h2>
        <p class="login__brand-description">
          Join thousands of teams using Zelo to manage their fleet efficiently.
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login__container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 100vh;
  background: var(--z-color-page);
}

/* Form Side */
.login__form-side {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--z-space-8);
}

.login__form-content {
  width: 100%;
  max-width: 400px;
}

.login__header {
  margin-bottom: var(--z-space-8);
}

.login__title {
  margin: 0 0 var(--z-space-2);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
  color: var(--z-color-text);
}

.login__subtitle {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.login__error {
  padding: var(--z-space-3);
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: var(--z-radius);
  font-size: var(--z-font-size-sm);
  color: #dc2626;
}

.login__form {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-4);
  margin-bottom: var(--z-space-6);
}

.login__field {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-2);
}

.login__label {
  font-size: var(--z-font-size-sm);
  font-weight: 500;
  color: var(--z-color-text);
}

.login__label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.login__forgot {
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
  text-decoration: none;
  transition: color 0.2s ease;
}

.login__forgot:hover {
  color: var(--z-color-text);
}

.login__button {
  width: 100%;
  margin-top: var(--z-space-2);
}

.login__signup {
  text-align: center;
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

.login__link {
  color: var(--z-color-text);
  text-decoration: none;
  font-weight: 500;
  transition: opacity 0.2s ease;
}

.login__link:hover {
  opacity: 0.7;
}

/* Brand Side */
.login__brand-side {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--z-space-8);
  background: linear-gradient(135deg, var(--z-series-1) 0%, var(--z-series-2) 100%);
  color: white;
}

.login__brand-content {
  max-width: 400px;
  text-align: center;
}

.login__brand-logo {
  font-size: var(--z-font-size-xl);
  font-weight: 600;
  margin-bottom: var(--z-space-8);
  opacity: 0.9;
}

.login__brand-title {
  margin: 0 0 var(--z-space-4);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
}

.login__brand-description {
  margin: 0;
  font-size: var(--z-font-size-sm);
  opacity: 0.9;
  line-height: 1.6;
}

@media (max-width: 900px) {
  .login__container {
    grid-template-columns: 1fr;
  }

  .login__brand-side {
    display: none;
  }

  .login__form-side {
    padding: var(--z-space-6);
  }
}

@media (max-width: 600px) {
  .login__form-side {
    padding: var(--z-space-4);
  }

  .login__form-content {
    max-width: 100%;
  }
}
</style>
