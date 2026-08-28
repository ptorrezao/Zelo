#!/usr/bin/env bash
# Gera os tipos TypeScript a partir do OpenAPI da Zelo.Api.
# Caminho de saida e sempre relativo a este script, nao ao cwd de quem o
# chama (pnpm --filter corre com cwd no package, nao na raiz do repo).
set -euo pipefail

SPEC_URL="${1:-http://localhost:8080/openapi/v1.json}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT="$SCRIPT_DIR/../../frontend/packages/api-client/src/schema.d.ts"

npx openapi-typescript "$SPEC_URL" -o "$OUT"
echo "Gerado: $OUT"
