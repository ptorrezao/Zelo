# Shadcn Components com Zelo Design System

Estes componentes são baseados em [shadcn/vue](https://shadcn-vue.com/) mas customizados para usar o design system Zelo.

## Vantagens

- ✅ Componentes acessíveis e bem testados
- ✅ Mantém o design visual do Zelo
- ✅ Usa Tailwind CSS (mais eficiente que CSS customizado)
- ✅ Facilita manutenção e consistência visual

## Componentes disponíveis

- **SButton** - Botão com variantes (primary, secondary, outline, destructive, ghost, link)
- **SCard** - Componente de card/painel
- **SCardHeader** - Header do card
- **SCardContent** - Conteúdo do card
- **SInput** - Campo de entrada com suporte a v-model
- **SAvatar** - Avatar com iniciais ou imagem

## Como usar

```vue
<script setup lang="ts">
import { SButton, SCard, SCardHeader, SCardContent, SInput } from '@zelo/ui/components/shadcn'
</script>

<template>
  <SCard>
    <SCardHeader>
      <h2>Título do Card</h2>
    </SCardHeader>
    <SCardContent>
      <SInput v-model="search" placeholder="Procurar..." />
      <SButton variant="primary">Enviar</SButton>
    </SCardContent>
  </SCard>
</template>
```

## Migração gradual

Pode usar componentes shadcn ao lado dos componentes customizados atuais. A migração é gradual:

1. Novos componentes usam shadcn
2. Componentes antigos mantêm-se funcionais
3. Migrar componentes conforme necessário

## Customização

O tema é definido em `tailwind.config.ts` usando as cores do Zelo:
- Cores: `--z-color-*`
- Espaçamento: `--z-space-*`
- Tipografia: `--z-font-*`
- Raios: `--z-radius-*`
