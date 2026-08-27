<script setup lang="ts">
definePageMeta({
  middleware: 'auth',
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
