<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

definePageMeta({
  layout: 'blank',
})

const router = useRouter()
const email = ref('')
const isLoading = ref(false)
const isSubmitted = ref(false)

async function handleSubmit() {
  isLoading.value = true
  try {
    // TODO: Implement actual forgot password API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    isSubmitted.value = true
  } finally {
    isLoading.value = false
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    handleSubmit()
  }
}
</script>

<template>
  <div class="forgot__container">
    <!-- Left: Form -->
    <div class="forgot__form-side">
      <div class="forgot__form-content">
        <a href="/login" class="forgot__back">← Back to sign in</a>

        <div class="forgot__header">
          <h1 class="forgot__title">Reset password</h1>
          <p class="forgot__subtitle">
            {{ isSubmitted ? 'Check your email' : 'Enter your email to receive a reset link' }}
          </p>
        </div>

        <div v-if="!isSubmitted" class="forgot__form">
          <div class="forgot__field">
            <label for="email" class="forgot__label">Email</label>
            <ZInput
              id="email"
              v-model="email"
              type="email"
              placeholder="seu@email.com"
              required
              @keydown="handleKeydown"
            />
          </div>

          <ZButton
            type="button"
            variant="primary"
            :disabled="isLoading"
            @click="handleSubmit"
            class="forgot__button"
          >
            {{ isLoading ? 'Sending...' : 'Send reset link' }}
          </ZButton>
        </div>

        <div v-else class="forgot__success">
          <p class="forgot__success-text">
            We've sent a password reset link to <strong>{{ email }}</strong>
          </p>
          <p class="forgot__success-hint">
            The link expires in 10 minutes.
          </p>
          <ZButton
            type="button"
            variant="primary"
            @click="() => router.push('/login')"
            class="forgot__button"
          >
            Back to sign in
          </ZButton>
        </div>
      </div>
    </div>

    <!-- Right: Branding -->
    <div class="forgot__brand-side">
      <div class="forgot__brand-content">
        <div class="forgot__brand-logo">Zelo</div>
        <h2 class="forgot__brand-title">Where teams ship together</h2>
        <p class="forgot__brand-description">
          Join thousands of teams using Zelo to manage their fleet efficiently.
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.forgot__container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 100vh;
  background: var(--z-color-page);
}

/* Form Side */
.forgot__form-side {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--z-space-8);
}

.forgot__form-content {
  width: 100%;
  max-width: 400px;
}

.forgot__back {
  display: inline-block;
  margin-bottom: var(--z-space-6);
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
  text-decoration: none;
  transition: color 0.2s ease;
}

.forgot__back:hover {
  color: var(--z-color-text);
}

.forgot__header {
  margin-bottom: var(--z-space-8);
}

.forgot__title {
  margin: 0 0 var(--z-space-2);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
  color: var(--z-color-text);
}

.forgot__subtitle {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
  line-height: 1.5;
}

.forgot__form {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-4);
}

.forgot__field {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-2);
}

.forgot__label {
  font-size: var(--z-font-size-sm);
  font-weight: 500;
  color: var(--z-color-text);
}

.forgot__button {
  width: 100%;
  margin-top: var(--z-space-2);
}

.forgot__success {
  display: flex;
  flex-direction: column;
  gap: var(--z-space-4);
}

.forgot__success-text {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text);
  line-height: 1.6;
}

.forgot__success-hint {
  margin: 0;
  font-size: var(--z-font-size-sm);
  color: var(--z-color-text-muted);
}

/* Brand Side */
.forgot__brand-side {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--z-space-8);
  background: linear-gradient(135deg, var(--z-series-1) 0%, var(--z-series-2) 100%);
  color: white;
}

.forgot__brand-content {
  max-width: 400px;
  text-align: center;
}

.forgot__brand-logo {
  font-size: var(--z-font-size-xl);
  font-weight: 600;
  margin-bottom: var(--z-space-8);
  opacity: 0.9;
}

.forgot__brand-title {
  margin: 0 0 var(--z-space-4);
  font-size: var(--z-font-size-xl);
  font-weight: 600;
}

.forgot__brand-description {
  margin: 0;
  font-size: var(--z-font-size-sm);
  opacity: 0.9;
  line-height: 1.6;
}

@media (max-width: 900px) {
  .forgot__container {
    grid-template-columns: 1fr;
  }

  .forgot__brand-side {
    display: none;
  }

  .forgot__form-side {
    padding: var(--z-space-6);
  }
}

@media (max-width: 600px) {
  .forgot__form-side {
    padding: var(--z-space-4);
  }

  .forgot__form-content {
    max-width: 100%;
  }
}
</style>
