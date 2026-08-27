import type { Config } from 'tailwindcss'
import defaultTheme from 'tailwindcss/defaultTheme'

export default {
  darkMode: ['class'],
  content: [
    './apps/**/*.{js,ts,vue}',
    './packages/**/*.{js,ts,vue}',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['system-ui', '-apple-system', '"Segoe UI"', 'Roboto', ...defaultTheme.fontFamily.sans],
      },
      colors: {
        // Usando o design system Zelo
        background: 'var(--z-color-bg)',
        foreground: 'var(--z-color-text)',
        card: 'var(--z-color-surface)',
        'card-foreground': 'var(--z-color-text)',
        popover: 'var(--z-color-surface)',
        'popover-foreground': 'var(--z-color-text)',
        muted: 'var(--z-color-surface)',
        'muted-foreground': 'var(--z-color-text-muted)',
        accent: 'var(--z-color-accent)',
        'accent-foreground': 'var(--z-color-accent-text)',
        destructive: '#dc2626',
        'destructive-foreground': '#fafafa',
        border: 'var(--z-color-border)',
        input: 'var(--z-color-bg)',
        ring: 'var(--z-color-accent)',
        primary: {
          DEFAULT: 'var(--z-color-accent)',
          foreground: 'var(--z-color-accent-text)',
        },
        secondary: {
          DEFAULT: 'var(--z-color-surface)',
          foreground: 'var(--z-color-text)',
        },
      },
      spacing: {
        0: '0',
        1: 'var(--z-space-1)',
        2: 'var(--z-space-2)',
        3: 'var(--z-space-3)',
        4: 'var(--z-space-4)',
        6: 'var(--z-space-6)',
        8: 'var(--z-space-8)',
      },
      fontSize: {
        xs: 'var(--z-font-size-xs)',
        sm: 'var(--z-font-size-sm)',
        base: 'var(--z-font-size-base)',
        lg: 'var(--z-font-size-lg)',
        xl: 'var(--z-font-size-xl)',
      },
      borderRadius: {
        lg: 'var(--z-radius-lg)',
        md: 'var(--z-radius)',
        sm: 'calc(var(--z-radius) / 2)',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
} satisfies Config
