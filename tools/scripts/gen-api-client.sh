#!/usr/bin/env bash
# Gera os tipos TypeScript a partir do OpenAPI da Zelo.Api.
set -euo pipefail

SPEC_URL="${1:-http://localhost:8080/openapi/v1.json}"
OUT="frontend/packages/api-client/src/schema.d.ts"

npx openapi-typescript "$SPEC_URL" -o "$OUT"
echo "Gerado: $OUT"
