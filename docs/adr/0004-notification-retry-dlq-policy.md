# ADR-004: Notification Retry and DLQ Policy

**Estado:** aceite
**Data:** 2026-09-15

## Contexto

Notificações (emails, push) podem falhar temporariamente. O sistema de filas já usa DLQs por fila,
mas não há política documentada para retries/backoff aplicável ao subsistema de notificações.

## Decisão

Definir política padrão para handlers de notificação:

- Número de tentativas: 3
- Backoff exponencial: base 5s (5s, 10s, 20s)
- Mensagens que excederem as tentativas são enviadas para a DLQ da fila específica
- Expor métricas e alertas para filas DLQ relacionadas a notificações

## Consequências

- Reduz risco de perda silenciosa de notificações temporariamente falhadas.
- Requer instrumentação de monitorização e dashboards.

## Alternativas

- Retries ilimitados: risco de filas bloqueadas.
- Retries com política manual por mensagem: complexidade operacional.
