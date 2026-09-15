# ADR-006: Background Job Registration Policy

**Estado:** aceite
**Data:** 2026-09-15

## Contexto

O repositório distingue dois hosts: `Zelo.Api` (apenas endpoints HTTP) e `Zelo.Worker` (consumers e jobs).
Sem política clara, é fácil registar services com comportamento indesejado no host errado.

## Decisão

Definir regra explícita: todos os `BackgroundService` e consumers que realizam trabalho assíncrono
de longa duração ou que dependem de filas só devem ser registados/instanciados no `Zelo.Worker`.

Implementar padrão: módulos expõem `AddXConsumers()` e `AddXBackgroundJobs()` onde for necessário;
o `Zelo.Worker` chama ambos; a `Zelo.Api` chama apenas os `AddXModule()` básicos.

## Consequências

- Evita correr jobs no processo da API (controlo e segurança).
- Torna explícito onde serviços de background vivem.

## Alternativas

- Manter abordagem ad-hoc: risco de regressões e trabalhos a correr na API.
