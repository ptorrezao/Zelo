// @vitest-environment happy-dom
import { computed, defineComponent, h, provide, ref } from 'vue'
import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import { SidebarContextKey, useSidebar, type SidebarContext } from './useSidebar'

function withProvider(context: SidebarContext, useIt: (ctx: SidebarContext) => void) {
  const Child = defineComponent({
    setup() {
      useIt(useSidebar())
      return () => h('div')
    },
  })
  const Parent = defineComponent({
    setup() {
      provide(SidebarContextKey, context)
      return () => h(Child)
    },
  })
  mount(Parent)
}

function newContext(): SidebarContext {
  const open = ref(true)
  return {
    state: computed(() => (open.value ? 'expanded' : 'collapsed')),
    open,
    setOpen: (value: boolean) => { open.value = value },
    isMobile: ref(false),
    openMobile: ref(false),
    setOpenMobile: () => {},
    toggleSidebar: () => { open.value = !open.value },
  }
}

describe('useSidebar', () => {
  it('lanca erro quando usado fora de um SidebarProvider', () => {
    const Broken = defineComponent({
      setup() {
        useSidebar()
        return () => h('div')
      },
    })

    expect(() => mount(Broken)).toThrow('useSidebar() só pode ser usado dentro de <SidebarProvider>')
  })

  it('devolve o contexto fornecido pelo SidebarProvider', () => {
    const context = newContext()
    let received: SidebarContext | undefined

    withProvider(context, (ctx) => { received = ctx })

    expect(received).toBe(context)
    expect(received?.state.value).toBe('expanded')
  })

  it('toggleSidebar alterna o estado open', () => {
    const context = newContext()

    context.toggleSidebar()

    expect(context.open.value).toBe(false)
    expect(context.state.value).toBe('collapsed')
  })
})
