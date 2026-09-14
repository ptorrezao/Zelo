import { useApiClient } from '@zelo/ui/composables/useApiClient'
import type { components } from '@zelo/api-client'

export type ImportHousehold = components['schemas']['HouseholdResponse']
export type ImportCandidateVehicle = components['schemas']['ImportCandidateVehicle']
export type ImportPreviewItem = components['schemas']['ImportPreviewItem']
export type ImportPreviewResponse = components['schemas']['ImportPreviewResponse']
export type ImportConfirmResponse = components['schemas']['ImportConfirmResponse']

export interface PreviewImportInput {
  destinationHouseholdId: string
  baseUrl: string
  email: string
  password: string
  remoteHouseholdId?: string
}

export function useImportVehicles() {
  const client = useApiClient()

  async function listMyHouseholds(): Promise<ImportHousehold[]> {
    const { data, error } = await client.GET('/api/v1/households/me')
    if (error || !data) {
      throw new Error('Não foi possível obter os seus households.')
    }
    return data
  }

  // Usado so para o aviso imediato no formulario (não pode importar de si
  // mesmo) - o backend faz a mesma verificação de qualquer forma, esta
  // chamada e só para não deixar o utilizador preencher tudo e submeter
  // primeiro.
  async function getMyEmail(): Promise<string | null> {
    const { data, error } = await client.GET('/api/auth/manage/info')
    if (error || !data) return null
    return data.email
  }

  async function previewImport(input: PreviewImportInput): Promise<ImportPreviewResponse> {
    const { data, error } = await client.POST('/api/v1/auto/vehicles/import/preview', {
      params: { query: { householdId: input.destinationHouseholdId } },
      body: {
        baseUrl: input.baseUrl,
        email: input.email,
        password: input.password,
        remoteHouseholdId: input.remoteHouseholdId ?? null,
      },
    })
    if (error) {
      throw new Error(error.error ?? 'Não foi possível ligar ao ambiente de origem.')
    }
    if (!data) {
      throw new Error('Não foi possível ligar ao ambiente de origem.')
    }
    return data
  }

  async function confirmImport(destinationHouseholdId: string, vehicles: ImportCandidateVehicle[]): Promise<ImportConfirmResponse> {
    const { data, error } = await client.POST('/api/v1/auto/vehicles/import/confirm', {
      params: { query: { householdId: destinationHouseholdId } },
      body: { vehicles },
    })
    if (error || !data) {
      throw new Error('Falha ao importar veículos.')
    }
    return data
  }

  return { listMyHouseholds, getMyEmail, previewImport, confirmImport }
}
