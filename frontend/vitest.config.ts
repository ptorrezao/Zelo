import { defineConfig } from 'vitest/config'

export default defineConfig({
  test: {
    include: ['apps/**/*.test.ts', 'packages/**/*.test.ts'],
    environment: 'node',
    coverage: {
      provider: 'v8',
      include: ['apps/**/utils/**/*.ts', 'apps/**/composables/**/*.ts', 'packages/**/composables/**/*.ts'],
      exclude: ['**/*.test.ts'],
    },
  },
})
