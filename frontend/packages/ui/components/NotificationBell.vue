<script setup lang="ts">
import { ref } from 'vue'
import { Bell, Check } from '@lucide/vue'
import { useNotifications } from '../composables/useNotifications'
import Sheet from './ui/Sheet.vue'
import SheetHeader from './ui/SheetHeader.vue'
import SheetTitle from './ui/SheetTitle.vue'
import Button from './ui/Button.vue'

defineOptions({ inheritAttrs: false })

const { notifications, unreadCount, acknowledge } = useNotifications()
const open = ref(false)

function formatDueOn(dueOn: string) {
  return new Date(dueOn).toLocaleDateString('pt-PT')
}
</script>

<template>
  <button
    type="button"
    v-bind="$attrs"
    class="relative inline-flex h-9 w-9 items-center justify-center rounded-md text-muted-foreground hover:bg-accent hover:text-accent-foreground"
    aria-label="Notificações"
    @click="open = true"
  >
    <Bell class="h-4 w-4" />
    <span
      v-if="unreadCount > 0"
      class="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-semibold text-destructive-foreground"
    >
      {{ unreadCount > 9 ? '9+' : unreadCount }}
    </span>
  </button>

  <Sheet :open="open" @update:open="open = $event">
    <SheetHeader>
      <SheetTitle>Notificações</SheetTitle>
    </SheetHeader>

    <div class="flex-1 overflow-y-auto">
      <p v-if="!notifications || notifications.length === 0" class="py-8 text-center text-sm text-muted-foreground">
        Sem notificações por ler.
      </p>
      <ul v-else class="flex flex-col gap-2">
        <li
          v-for="item in notifications"
          :key="item.id"
          class="flex items-start justify-between gap-3 rounded-md border border-border p-3 text-sm"
        >
          <div>
            <p class="font-medium text-foreground">{{ item.title }}</p>
            <p class="text-muted-foreground">
              Vence a {{ formatDueOn(item.dueOn) }}
              <template v-if="item.daysUntilDue > 0">(daqui a {{ item.daysUntilDue }} dias)</template>
              <template v-else>(hoje)</template>
            </p>
          </div>
          <Button variant="ghost" size="icon" aria-label="Marcar como lida" @click="acknowledge(item.id)">
            <Check class="h-4 w-4" />
          </Button>
        </li>
      </ul>
    </div>
  </Sheet>
</template>
