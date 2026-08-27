<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { cn } from '../../lib/utils'

interface Props {
  modelValue: string // formato DD/MM/AAAA
  placeholder?: string
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'Selecionar data',
})

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const WEEKDAYS = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S']
const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

function parseDate(value: string): Date | null {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(value)
  if (!match) return null
  const [, day, month, year] = match
  return new Date(Number(year), Number(month) - 1, Number(day))
}

function formatDate(date: Date): string {
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  return `${day}/${month}/${date.getFullYear()}`
}

const open = ref(false)
const root = ref<HTMLElement>()

const selectedDate = computed(() => parseDate(props.modelValue))
const viewDate = ref(selectedDate.value ?? new Date())

const daysInGrid = computed(() => {
  const year = viewDate.value.getFullYear()
  const month = viewDate.value.getMonth()
  const firstDay = new Date(year, month, 1)
  const startOffset = firstDay.getDay()
  const daysInMonth = new Date(year, month + 1, 0).getDate()

  const days: { date: Date, outside: boolean }[] = []
  for (let i = 0; i < startOffset; i++) {
    days.push({ date: new Date(year, month, i - startOffset + 1), outside: true })
  }
  for (let d = 1; d <= daysInMonth; d++) {
    days.push({ date: new Date(year, month, d), outside: false })
  }
  while (days.length % 7 !== 0) {
    const last = days[days.length - 1].date
    days.push({ date: new Date(last.getFullYear(), last.getMonth(), last.getDate() + 1), outside: true })
  }
  return days
})

function isSameDay(a: Date, b: Date | null) {
  return !!b && a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

function toggle() {
  if (!open.value) {
    viewDate.value = selectedDate.value ?? new Date()
  }
  open.value = !open.value
}

function pick(date: Date) {
  emit('update:modelValue', formatDate(date))
  open.value = false
}

function changeMonth(offset: number) {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() + offset, 1)
}

function handleClickOutside(event: MouseEvent) {
  if (root.value && !root.value.contains(event.target as Node)) {
    open.value = false
  }
}

onMounted(() => document.addEventListener('click', handleClickOutside))
onBeforeUnmount(() => document.removeEventListener('click', handleClickOutside))
</script>

<template>
  <div ref="root" class="relative">
    <button
      type="button"
      :class="cn(
        'flex h-10 w-full items-center gap-2 rounded-md border border-input bg-background px-3 py-2 text-left text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
        !modelValue ? 'text-muted-foreground' : '',
        props.class,
      )"
      @click="toggle"
    >
      <svg class="h-4 w-4 shrink-0 opacity-60" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <rect x="3" y="4" width="18" height="18" rx="2" />
        <path d="M16 2v4M8 2v4M3 10h18" stroke-linecap="round" />
      </svg>
      {{ modelValue || placeholder }}
    </button>

    <div
      v-if="open"
      class="absolute z-20 mt-1 w-64 rounded-md border border-border bg-popover p-3 text-popover-foreground shadow-md"
    >
      <div class="mb-2 flex items-center justify-between">
        <button type="button" class="rounded-sm p-1 hover:bg-accent" @click="changeMonth(-1)">
          <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="m15 18-6-6 6-6" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>
        <span class="text-sm font-medium">{{ MONTHS[viewDate.getMonth()] }} {{ viewDate.getFullYear() }}</span>
        <button type="button" class="rounded-sm p-1 hover:bg-accent" @click="changeMonth(1)">
          <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="m9 18 6-6-6-6" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>
      </div>

      <div class="grid grid-cols-7 gap-1 text-center text-xs text-muted-foreground">
        <span v-for="(day, idx) in WEEKDAYS" :key="idx">{{ day }}</span>
      </div>
      <div class="mt-1 grid grid-cols-7 gap-1">
        <button
          v-for="(day, idx) in daysInGrid"
          :key="idx"
          type="button"
          :class="[
            'flex h-8 w-8 items-center justify-center rounded-md text-sm hover:bg-accent',
            day.outside ? 'text-muted-foreground/50' : '',
            isSameDay(day.date, selectedDate) ? 'bg-primary text-primary-foreground hover:bg-primary' : '',
          ]"
          @click="pick(day.date)"
        >
          {{ day.date.getDate() }}
        </button>
      </div>
    </div>
  </div>
</template>
