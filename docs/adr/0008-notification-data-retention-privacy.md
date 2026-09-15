# ADR-008: Notification Data Retention and Privacy

**Estado:** aceite
**Data:** 2026-09-15

## Contexto

Notification logs contain PII (household identifiers, may include vehicle plates or emails).
Sem política clara de retenção e minimização, há risco de exposição e aumento de custo de armazenamento.

## Decisão

Adotar política padrão:

- Retenção primária: 90 dias em `core.notification_logs`.
- Arquivo chunked para armazenamento frio após 90 dias ou anonimização parcial.
- Minimizar PII no `notification_logs` (guardar IDs e resumo textual reduzido, evitar corpos/HTML completos).

## Consequências

- Menor superfície de dados sensíveis e custo reduzido.
- Requer processo de arquivamento e políticas de acesso.

## Alternativas

- Manter logs indefinidamente — mais simples, maior risco e custo.
