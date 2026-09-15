# Plano: Anexar Documentos a um Veículo

> **Estado: essencial implementado** — o backend (upload presigned URL, criar/listar/apagar documentos) já existia em `Zelo.Modules.Auto`. Faltava mesmo a UI de anexar: `frontend/apps/auto/pages/documentos.vue` agora tem o fluxo completo (escolher categoria + ficheiro → pedir URL pré-assinado → PUT direto para o Garage → confirmar o registo). Foi preciso também configurar CORS no bucket (`GarageBootstrap.EnsureBucketCorsAsync`, via `PutBucketCors` — o upload do browser é sempre cross-origin), sem isso o PUT falhava em silêncio. Extras do plano original (checksum, antivírus, audit log, thumbnails, soft-delete, lifecycle rules) ficam fora de âmbito — não existe nada equivalente no resto do projeto, e não foram pedidos.

## Objetivo
- Permitir anexar/associar documentos (PDF, imagens, etc.) à entidade `Vehicle` de forma segura, audível, pesquisável e escalável.

## Escopo inicial
- Endpoint de upload no backend (presigned URL), persistência de metadados, armazenamento (S3/local), UI mínima para anexar, e testes automatizados.

## Passos principais
1. Definir requisitos e regras de negócio.
2. Modelar dados e criar migration.
3. Implementar armazenamento de ficheiros (S3 ou local).
4. Implementar API: upload, list, download, delete.
5. Validar e processar uploads (checksum, sanitização, antivirus opcional).
6. Autenticação, autorização e auditoria.
7. UI/UX: fluxo de anexar com feedback.
8. Testes (unitários, integração, E2E).
9. Deploy com feature flag e rollout gradual.
10. Monitorização e alertas.

## Requisitos e regras de negócio
- Tipos permitidos: PDF, JPG, PNG (configurável).
- Tamanho máximo por ficheiro: 10 MB (configurável).
- Metadados obrigatórios: `vehicleId`, `documentType`, `uploadedBy`, `uploadedAt`.
- Permissões: apenas utilizadores autorizados podem anexar/remover; leitura conforme regra de acesso.
- Retenção: política para arquivamento/exclusão (se aplicável).

## Modelagem de dados (exemplo)
- Tabela `VehicleDocuments`:
  - `id` (UUID)
  - `vehicleId` (FK)
  - `filename`
  - `contentType`
  - `size`
  - `storageKey` / `storageUrl`
  - `documentType`
  - `uploadedBy`
  - `uploadedAt`
  - `checksum` (SHA256)
  - `deletedAt` (soft delete)
  - `metadata` (JSON)
- Índices: `vehicleId`, `documentType`.

## API / Contratos (exemplo)
- `POST /vehicles/{id}/documents` — iniciar upload (retorna presigned URL ou aceita multipart).
- `GET /vehicles/{id}/documents` — listar metadados.
- `GET /vehicles/{id}/documents/{docId}` — baixar (redirect para storage ou streaming).
- `DELETE /vehicles/{id}/documents/{docId}` — soft-delete.

Respostas: retornar `id`, `storageKey`, `filename`, `uploadedAt`.
Erros: 400 validação, 401/403 auth, 413 payload grande, 415 tipo não suportado, 500 erro servidor.

## Armazenamento
- Recomenda-se S3 ou compatível: presigned URLs para uploads diretos.
- Estrutura de keys: `vehicles/{vehicleId}/{uuid}_{sanitizedFilename}`.
- Configurar lifecycle rules (arquivar / expirar) para controlar custos.

## Validação e processamento
- Validar lado servidor: content-type, extensão, tamanho, checksum.
- Sanitizar nomes para evitar path traversal.
- Opcional: gerar thumbnails, OCR para indexação, extrair metadados.
- Opcional: integração com scanner antivirus (e.g., ClamAV).

## Autenticação, autorização e auditoria
- Autenticação: JWT/OAuth com scopes apropriados.
- Autorização: roles/policies (ex.: `vehicle:document:create`).
- Auditoria: registar eventos em `AuditLog` (actor, ação, docId, timestamp).

## UI/UX (fluxo sugerido)
- Botão “Adicionar documento” no detalhe do veículo.
- Modal com campos: `documentType`, `descrição` (opcional) e upload.
- Progresso do upload, preview para imagens e mensagens de erro amigáveis.

## Testes
- Unitários: validação, geração de storageKey, lógica de metadados.
- Integração: fluxo de presigned URL, armazenamento e leitura.
- E2E: anexar e descarregar via UI.
- Segurança: testes para uploads maliciosos (content-sniffing, path traversal).

## Migração e deploy
- Criar migration incremental para `VehicleDocuments`.
- Deploy com feature flag (canary) e monitorização.

## Monitorização e operações
- Logs: registar uploads, falhas e operações de auditoria.
- Métricas: número de uploads, latências, erros.
- Alertas: taxa de erros elevada, storage perto do limite.

## Critérios de aceitação
- Upload válido é persistido e associado ao `vehicleId`.
- Metadados estão corretos e são listáveis.
- Apenas utilizadores autorizados conseguem criar/remover.
- Download devolve ficheiro íntegro (checksum OK).
- Cobertura de testes automatizados para validação e fluxo principal.

## Riscos e mitigação
- Upload malicioso: validar tipo, integrar antivirus.
- Custos de storage: aplicar lifecycle / compressão.
- Escalabilidade: usar uploads diretos a S3 e CDN para downloads.

---

Se quiser, implemento a migration e o endpoint `POST /vehicles/{id}/documents` com suporte a presigned URLs e testes básicos.
