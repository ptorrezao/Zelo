import { ref } from 'vue'
import { useApiClient } from '@zelo/ui/composables/useApiClient'

export type VehicleCatalog = Record<string, string[]>

// Catalogo de marcas/modelos - vem do backend (GET /api/auto/vehicle-catalog,
// servido a partir de um JSON embutido no servidor) para ser facil de
// manter sem publicar uma nova versao do frontend. So serve de sugestao
// (datalist) nos campos de Marca/Modelo, que continuam a aceitar texto
// livre - nunca bloqueia quem tem uma marca/modelo fora desta lista.
const catalog = ref<VehicleCatalog>({})
let loadPromise: Promise<void> | null = null

async function load(client: ReturnType<typeof useApiClient>) {
  const { data } = await client.GET('/api/auto/vehicle-catalog')
  catalog.value = data ?? {}
}

export function useVehicleCatalog() {
  const client = useApiClient()

  if (!loadPromise) {
    loadPromise = load(client)
  }

  return { catalog }
}
