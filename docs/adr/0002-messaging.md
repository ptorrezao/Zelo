# ADR-002: Bus de eventos atrás de abstração própria

**Estado:** aceite

## Decisão

Toda a mensageria passa por `IEventPublisher` / `IEventHandler<T>`,
definidos em `Zelo.Messaging`. A biblioteca concreta vive apenas em
`Zelo.Messaging/Internal/` e não é referenciada por mais nenhum projeto.

## Porquê

O transporte vai mudar: in-process em desenvolvimento, filas sobre
PostgreSQL na primeira fase, broker dedicado se e quando houver carga.
Nenhuma dessas mudanças deve tocar em código de módulo.

Acresce a questão de licenciamento das bibliotecas maduras do ecossistema
.NET, que mudou nos últimos anos — a abstração mantém a troca barata.

## Regras

- nenhum `using` da biblioteca de mensageria fora de `Zelo.Messaging`
- verificado por teste de arquitetura
- outbox transacional no mesmo `DbContext` da alteração de estado
- consumidores idempotentes: entrega é *pelo menos uma vez*
