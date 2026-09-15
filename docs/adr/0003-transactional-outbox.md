# ADR-003: Transactional Outbox for Events

**Estado:** aceite
**Data:** 2026-09-15

## Contexto

O sistema exige consistência entre escritas de estado (ex.: gravar que um lembrete foi criado/registrado)
e a publicação do evento que notifica outros consumidores. ADR-002 exige outbox transacional, mas
não há uma decisão formal sobre a sua adopção neste caso.

## Decisão

Implementar um *transactional outbox* no mesmo `DbContext` do `core` para garantir atomicidade
entre a gravação do `notification_log` e o envio do evento `ObligationReminderDue`. O fluxo é:

- Escrever `notification_log` e uma linha na tabela `core.outbox_messages` numa única transação.
- Um processo outbox (no Worker ou serviço dedicado) lê mensagens committed da tabela e publica-as para RabbitMQ.

Sugestão de esquema para a tabela `core.outbox_messages`:

```sql
CREATE TABLE core.outbox_messages (
	id uuid PRIMARY KEY,
	occurred_at timestamptz NOT NULL,
	event_type text NOT NULL,
	payload jsonb NOT NULL,
	attempts int NOT NULL DEFAULT 0,
	processed_at timestamptz NULL,
	locked_until timestamptz NULL
);
```

Fluxo operacional resumido:

- Produtor: dentro da transação que escreve `notification_log`, inserir uma linha em `outbox_messages` com `processed_at = NULL`.
- Outbox publisher: seleciona mensagens com `processed_at IS NULL` e `locked_until < now()`, marca `locked_until = now() + lease`, publica para RabbitMQ e, em caso de sucesso, define `processed_at = now()`; em falha incrementa `attempts` e limpa `locked_until`.

Idempotência do consumidor e retentativa do outbox são complementares: o outbox garante que a mensagem será tentada até ser processada, enquanto os handlers devem ser idempotentes para tolerar reentregas.

## Consequências

- Garante atomicidade e evita perder notificações quando a publicação falha após commit.
- Introduz complexidade operacional (tarefa outbox e tabela adicional).
- Facilita replays e auditoria das mensagens publicadas.

## Alternativas

- Publicar directamente via `IEventPublisher` sem outbox: mais simples mas sujeito a inconsistências.
- Usar CDC (Debezium) ou um broker transacional suportado pelo cloud provider — reduz código mas aumenta complexidade operacional.
