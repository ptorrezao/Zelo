import { inject, type ComputedRef, type InjectionKey, type Ref } from 'vue'

export const SIDEBAR_COOKIE_NAME = 'sidebar_state'
export const SIDEBAR_COOKIE_MAX_AGE = 60 * 60 * 24 * 7
export const SIDEBAR_WIDTH = '16rem'
export const SIDEBAR_WIDTH_MOBILE = '18rem'
export const SIDEBAR_WIDTH_ICON = '3rem'
export const SIDEBAR_KEYBOARD_SHORTCUT = 'b'

export interface SidebarContext {
  state: ComputedRef<'expanded' | 'collapsed'>
  open: Ref<boolean>
  setOpen: (open: boolean) => void
  isMobile: Ref<boolean>
  openMobile: Ref<boolean>
  setOpenMobile: (open: boolean) => void
  toggleSidebar: () => void
}

export const SidebarContextKey: InjectionKey<SidebarContext> = Symbol('SidebarContext')

export function useSidebar(): SidebarContext {
  const context = inject(SidebarContextKey)
  if (!context) {
    throw new Error('useSidebar() só pode ser usado dentro de <SidebarProvider>')
  }
  return context
}
