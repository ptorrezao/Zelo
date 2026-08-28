import { describe, expect, it, vi } from 'vitest'

const useFetchMock = vi.fn()

vi.mock('#app', () => ({
  useFetch: useFetchMock,
}))

describe('useFeatureFlags', () => {
  it('chama useFetch com a key e o fallback corretos', async () => {
    useFetchMock.mockReturnValue(Promise.resolve({ data: { value: { autoAppEnabled: true, inventoryAppEnabled: true } } }))

    const { useFeatureFlags } = await import('./useFeatureFlags')
    await useFeatureFlags()

    expect(useFetchMock).toHaveBeenCalledWith('/api/feature-flags', expect.objectContaining({ key: 'feature-flags' }))
    const options = useFetchMock.mock.calls[0][1]
    expect(options.default()).toEqual({ autoAppEnabled: true, inventoryAppEnabled: true })
  })
})
