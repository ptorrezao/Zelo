<script setup lang="ts">
import '@zelo/ui/styles/global.css'

definePageMeta({
  middleware: 'auth',
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
