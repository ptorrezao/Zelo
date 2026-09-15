# Fix: card de manutenção não busca dados ao trocar de veículo

> **Estado: implementado** — loadVehicleDetail em useVehicles.ts passou a usar detailLoaded/detailPromises em vez do tamanho dos arrays; teste de regressao adicionado em useVehicles.test.ts.

## Contexto

Este bug já foi "corrigido" no commit `2c9103e` (troca de um `if` direto por
`watch(selected, ..., { immediate: true })` em [useVehicles.ts:189-191](frontend/apps/auto/composables/useVehicles.ts#L189-L191)).
O sintoma reportado agora persiste, portanto o watch não é (ou já não é) a
causa — há outra coisa a bloquear o fetch.

## Causa raiz identificada

[useVehicles.ts:144](frontend/apps/auto/composables/useVehicles.ts#L144), em `loadVehicleDetail`:

```ts
if (vehicle.maintenances.length > 0 || vehicle.documents.length > 0) return // ja carregado
```

Usa o *tamanho do array* como proxy para "já carreguei os detalhes deste
veículo". Isto parte-se em qualquer caminho que meta algo no array antes do
fetch real acontecer — o único caso que já existe no código é
`addMaintenance` ([useVehicles.ts:311-314](frontend/apps/auto/composables/useVehicles.ts#L311-L314)):

```ts
const maintenance = mapMaintenanceFromApi(data)
const vehicle = allVehicles.value.find(v => v.id === vehicleId)
vehicle?.maintenances.unshift(maintenance)
```

Sequência que reproduz o bug:
1. Utilizador seleciona o veículo A (ainda sem detalhe carregado).
2. Antes do GET de manutenções resolver, adiciona uma manutenção nova a A
   (`AddMaintenanceSheet` → `addMaintenance`). `vehicle.maintenances` passa a
   ter 1 item.
3. Quando o `watch(selected, ...)` (ou uma nova seleção do mesmo veículo)
   chama `loadVehicleDetail` outra vez, a guarda vê `length > 0` e **desiste
   sem pedir nada à API** — o histórico real de manutenções nunca chega a
   ser buscado, só se vê o item acabado de criar.

Também é possível (mas secundário) que ao trocar de veículo os vários
componentes que chamam `useVehicles()` (`index.vue`, `VehicleListCard`,
`MaintenanceCard`, `VehicleHeader`, `VehicleDetailsCard`, `VehicleStatsCard`)
disparem `loadVehicleDetail` em paralelo antes do primeiro `await` resolver,
gerando pedidos duplicados — não impede o fetch, mas é desperdício que a
mesma correção resolve de graça.

## Correção proposta

Substituir a heurística por tamanho de array por uma flag explícita de
"detalhe carregado", guardada fora do array de dados — e cachear a própria
promise em curso para eliminar os pedidos duplicados entre componentes.

```ts
const detailLoaded = new Set<string>()
const detailPromises = new Map<string, Promise<void>>()

async function loadVehicleDetail(client, vehicle) {
  if (detailLoaded.has(vehicle.id)) return
  const inFlight = detailPromises.get(vehicle.id)
  if (inFlight) return inFlight

  const promise = (async () => {
    const [maintenances, documents] = await Promise.all([...])
    vehicle.maintenances = (maintenances.data ?? []).map(mapMaintenanceFromApi)
    vehicle.documents = (documents.data ?? []).map(mapDocumentFromApi)
    detailLoaded.add(vehicle.id)
  })()

  detailPromises.set(vehicle.id, promise)
  try {
    await promise
  } finally {
    detailPromises.delete(vehicle.id)
  }
}
```

Pontos a rever ao implementar:
- `refresh()` recria os veículos (objetos novos) — limpar `detailLoaded` /
  `detailPromises` nesse momento (ou basear o Set em `vehicleId`, que muda
  de qualquer forma porque os IDs vêm da API, não é reaproveitado).
- `addMaintenance` deixa de precisar de se preocupar com o array — nenhuma
  mudança necessária aí, o bug estava só na guarda.

## Teste

Adicionar um caso a [useVehicles.test.ts](frontend/apps/auto/composables/useVehicles.test.ts)
que reproduz a sequência: `addMaintenance` num veículo antes do detalhe
carregar, depois forçar novo `loadVehicleDetail` (ex.: reselecionar o
veículo) e confirmar que o GET de manutenções é chamado e que o resultado
final inclui tanto o item mockado pela API como o adicionado.

## Fora de âmbito

- Deduplicar chamadas entre múltiplas instâncias de `useVehicles()` é
  resolvido de borla pela mesma alteração (in-flight promise cache); não
  precisa de trabalho à parte.
- Não mexer no mecanismo de `watch(selected, ...)` — está correto.
