# Plano de Implementação: Notificações baseadas em Datas de Veículos

## 1. Visão Geral

Implementar um sistema de notificações que alerta os utilizadores sobre eventos futuros relacionados com veículos (seguro, inspeção, manutenção, etc.) com base em datas configuráveis.

**Exemplo**: "Dentro de 15 dias o seguro vai expirar"

---

## 2. Objetivos

- ✅ Notificar utilizadores antecipadamente sobre datas importantes
- ✅ Permitir configuração de períodos de aviso personalizados
- ✅ Suportar múltiplos tipos de eventos (seguro, inspeção, manutenção, revisão)
- ✅ Fornecer notificações via diferentes canais (in-app, email, push)
- ✅ Manter histórico de notificações entregues
- ✅ Permitir ao utilizador gerir preferências de notificação

---

## 3. Quem envia as notificações? (ponto crítico)

Hoje **ninguém envia nada** — só existe envio de email para fluxos de autenticação
(confirmação de conta, reset de password), feito por `SmtpEmailSender` dentro de
`Zelo.Modules.Identity` (ver [SmtpEmailSender.cs](../backend/src/Modules/Zelo.Modules.Identity/Infrastructure/SmtpEmailSender.cs)).
Essa classe é `internal` ao módulo Identity e está acoplada a `IEmailSender<ZeloUser>` do
ASP.NET Core Identity — não dá para reutilizar diretamente para notificações de veículos.
Não existe qualquer envio de push nem mecanismo de "in-app" hoje.

Este projeto já é um monólito modular orientado a eventos (RabbitMQ via `Zelo.Messaging`),
com dois hosts:
- **`Zelo.Api`** — só regista os módulos e expõe endpoints HTTP. **Nunca corre consumers nem jobs.**
- **`Zelo.Worker`** — regista os mesmos módulos + `AddXConsumers()` de cada um. É o único
  sítio onde correm handlers de eventos (`IEventHandler<T>`) e onde deve correr qualquer
  job periódico em background.

**Conclusão prática**: o *emissor real* das notificações vai ser o **`Zelo.Worker`**, através de
dois componentes novos que vivem lá:

1. **Um `BackgroundService` periódico (o "produtor")** — corre por ex. de hora a hora, olha
   para as datas relevantes na base de dados e, para cada uma que atinja o limiar de aviso,
   publica um evento de integração via `IEventPublisher` (o mesmo mecanismo já usado por
   `ObligationScheduled`, `AssetCreated`, etc.).
2. **Um `IEventHandler<T>` (o "consumidor")** — regista-se via `AddZeloEventHandler<TEvent,THandler>`
   como qualquer outro handler do projeto, recebe o evento pela fila própria do RabbitMQ, e é
   **aí** que o envio de facto acontece: chama um `IEmailSender` novo (não o de Identity) para
   mandar o email, e/ou grava uma linha "por ler" para o utilizador ver in-app.

Ou seja: produtor e consumidor até podem ficar no mesmo host (`Zelo.Worker`), mas estão
desacoplados por uma fila — se o envio de email falhar, a mensagem vai para a DLQ em vez de
perder-se ou bloquear a próxima verificação periódica.

### 3.1 Reutilizar o motor de obrigações existente (Core)

O `Zelo.Modules.Core` já tem exatamente a peça que falta: a tabela `obligations`
(`HouseholdId`, `AssetId`, `Module`, `Title`, `DueOn`, `CompletedOn`) é alimentada pelos outros
módulos via `ObligationScheduled` / `ObligationUpdated` / `ObligationCompleted`. O `Auto` já
publica isto para a próxima inspeção do veículo (ver
[VehicleEvents.cs](../backend/src/Modules/Zelo.Modules.Auto/Application/VehicleEvents.cs)).
Isto quer dizer que **não é preciso criar uma tabela paralela `VehicleNotificationRules`** —
basta:

- Garantir que o Auto publica `ObligationScheduled`/`ObligationUpdated` também para o seguro
  (hoje só existe para a inspeção — é preciso estender `VehicleEvents` e o(s) campo(s) do
  `Vehicle` com a data de expiração do seguro, se ainda não existir).
- Adicionar ao `Core` a lógica de "lembrete": um `BackgroundService` que corre no Worker,
  consulta `core.obligations WHERE CompletedOn IS NULL AND DueOn <= today + DaysWarning`, e
  publica um novo evento `ObligationReminderDue` (uma vez por obligação, com deduplicação).
- Um handler novo (`ObligationReminderDueHandler`) subscreve esse evento e envia o email /
  regista a notificação in-app.

### 3.2 Componentes novos (backend)

```
Zelo.Modules.Core/
├── Domain/
│   └── NotificationLog.cs                 (histórico + deduplicação)
├── Application/
│   └── NotificationSettings.cs            (dias de aviso, configurável)
├── Consumers/
│   └── ObligationReminderDueHandler.cs     (consumidor — envia o email)
├── Infrastructure/
│   ├── ObligationReminderCheckService.cs   (BackgroundService — produtor, só no Worker)
│   └── SmtpNotificationSender.cs           (novo sender, independente do de Identity)
└── Endpoints/
    └── NotificationEndpoints.cs            (listar/marcar como lida)
```

Novo evento em `Zelo.Contracts`:

```csharp
public sealed record ObligationReminderDue(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ObligationId,
    Guid HouseholdId,
    string Title,
    DateOnly DueOn,
    int DaysUntilDue) : IIntegrationEvent;
```

### 3.3 Base de Dados (schema Postgres, coerente com o resto do projeto)

Só é preciso acrescentar o essencial — reaproveitando `core.obligations`:

```sql
-- Regista que já foi enviado um lembrete para esta obrigação (evita duplicar
-- notificações em cada corrida do BackgroundService).
CREATE TABLE core.notification_logs (
    id uuid PRIMARY KEY,
    obligation_id uuid NOT NULL REFERENCES core.obligations(id),
    household_id uuid NOT NULL,
    days_until_due int NOT NULL,
    triggered_at timestamptz NOT NULL,
    acknowledged_at timestamptz NULL
);

-- Preferências de notificação por household (dias de aviso, canais ativos).
CREATE TABLE core.notification_preferences (
    household_id uuid PRIMARY KEY,
    days_warning int NOT NULL DEFAULT 15,
    email_enabled boolean NOT NULL DEFAULT true,
    push_enabled boolean NOT NULL DEFAULT false
);
```

---

## 4. Fluxo de Funcionamento

### 4.1 Configuração Inicial

1. O utilizador já define a data de inspeção/seguro no formulário do veículo existente
   (`Zelo.Modules.Auto`) — não é preciso um ecrã novo de "regras de notificação".
2. O `Auto` publica/atualiza a obrigação correspondente (`ObligationScheduled`/`Updated`),
   como já faz hoje para a inspeção.
3. Opcionalmente, o utilizador ajusta `days_warning` nas preferências do household.

### 4.2 Verificação Periódica — quem "descobre" que está na hora (produtor)

`ObligationReminderCheckService` (`BackgroundService`, só registado no `Zelo.Worker`):

```
a cada 1 hora:
    hoje = DateOnly hoje
    para cada obligation em core.obligations
         onde CompletedOn IS NULL
         e DueOn <= hoje + DaysWarning (por household)
         e NÃO existe já um notification_log para esta obligation:

        publicar ObligationReminderDue(obligationId, householdId, title, dueOn, diasRestantes)
```

Exemplo: seguro expira em 2026-09-30, aviso a 15 dias → a partir de 2026-09-15 o evento
é publicado (uma única vez, graças ao registo em `notification_logs`).

### 4.3 Entrega — quem efetivamente envia (consumidor)

`ObligationReminderDueHandler : IEventHandler<ObligationReminderDue>` (só corre no `Zelo.Worker`,
como qualquer outro handler, registado por `AddCoreConsumers`):

1. Regista a linha em `core.notification_logs` (marca como já notificado).
2. Envia o email através de um `INotificationEmailSender` novo (não o `SmtpEmailSender` do
   Identity — esse é interno e específico de `ZeloUser`; aqui basta reaproveitar o mesmo padrão
   SMTP + Mailhog em dev).
3. (V1.1) Publica também para um canal in-app / push, quando existirem.

Se o envio falhar, a mensagem RabbitMQ vai para a DLQ própria (`core.obligationreminderdue.dlq`)
em vez de bloquear ou perder-se — mesmo comportamento que os outros handlers do projeto.

---

## 5. Especificações Técnicas

### 5.1 Entidades de Domínio (novas, em `Zelo.Modules.Core`)

```csharp
// Domain/NotificationLog.cs
internal sealed class NotificationLog
{
    public Guid Id { get; init; }
    public Guid ObligationId { get; init; }
    public Guid HouseholdId { get; set; }
    public int DaysUntilDue { get; set; }
    public DateTimeOffset TriggeredAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
}

// Domain/NotificationPreference.cs
internal sealed class NotificationPreference
{
    public Guid HouseholdId { get; init; }
    public int DaysWarning { get; set; } = 15;
    public bool EmailEnabled { get; set; } = true;
    public bool PushEnabled { get; set; }
}
```

### 5.2 Produtor — `ObligationReminderCheckService` (só no Worker)

```csharp
// Infrastructure/ObligationReminderCheckService.cs
internal sealed class ObligationReminderCheckService(
    IServiceScopeFactory scopeFactory,
    IEventPublisher events,
    TimeProvider clock) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        do
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
            var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

            var due = await db.Obligations
                .Where(o => o.CompletedOn == null)
                .Where(o => !db.NotificationLogs.Any(l => l.ObligationId == o.Id))
                .Where(o => o.DueOn <= today.AddDays(15)) // TODO: por household (NotificationPreference)
                .ToListAsync(ct);

            foreach (var obligation in due)
            {
                var daysUntil = obligation.DueOn.DayNumber - today.DayNumber;
                await events.PublishAsync(new ObligationReminderDue(
                    Guid.NewGuid(), clock.GetUtcNow(), obligation.Id, obligation.HouseholdId,
                    obligation.Title, obligation.DueOn, daysUntil), ct);
            }
        } while (await timer.WaitForNextTickAsync(ct));
    }
}
```

### 5.3 Consumidor — quem envia de facto

```csharp
// Consumers/ObligationReminderDueHandler.cs
internal sealed class ObligationReminderDueHandler(
    CoreDbContext db,
    INotificationEmailSender emailSender) : IEventHandler<ObligationReminderDue>
{
    public async Task HandleAsync(ObligationReminderDue @event, CancellationToken ct)
    {
        if (await db.NotificationLogs.AnyAsync(l => l.ObligationId == @event.ObligationId, ct))
            return; // idempotencia - a fila pode reentregar a mesma mensagem

        db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            ObligationId = @event.ObligationId,
            HouseholdId = @event.HouseholdId,
            DaysUntilDue = @event.DaysUntilDue,
            TriggeredAt = @event.OccurredAt,
        });
        await db.SaveChangesAsync(ct);

        await emailSender.SendReminderAsync(@event.HouseholdId, @event.Title, @event.DaysUntilDue, ct);
    }
}
```

Registo em `CoreModule.AddCoreConsumers`:
```csharp
services.AddZeloEventHandler<ObligationReminderDue, ObligationReminderDueHandler>("core.obligationreminderdue");
```
E em `CoreModule.AddCoreModule` (só no Worker, tal como o `ObligationReminderCheckService` — a
Api nunca deve correr jobs, ver `Program.cs` da Api vs. Worker).

### 5.4 API Endpoints (apenas leitura/preferências — não há "criar regra")

```
GET  /api/households/{householdId}/notifications          → notificações pendentes/recentes
POST /api/notifications/{notificationLogId}/acknowledge    → marcar como vista
GET  /api/households/{householdId}/notifications/preferences
PUT  /api/households/{householdId}/notifications/preferences   { daysWarning, emailEnabled, pushEnabled }
```

### 5.5 Novo evento em `Zelo.Contracts`

```csharp
public sealed record ObligationReminderDue(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ObligationId,
    Guid HouseholdId,
    string Title,
    DateOnly DueOn,
    int DaysUntilDue) : IIntegrationEvent;
```


### 5.6 Provider de geração de imagens (feature flag)

A geração de imagens associada a veículos deve ser tratada como uma decisão operacional de produto, não como configuração de infraestrutura.

O módulo `Zelo.Modules.Auto` já abstrai este caso com `IVehicleImageGenerator` em
`Infrastructure/IVehicleImageGenerator.cs`: isto é o ponto correto para trocar o fornecedor sem mexer no handler.

Sugestão de desenho:

- `VehicleImageProvider`: `Disabled`, `OpenAi`, `Gemini`
- `IVehicleImageGeneratorSelector` / `VehicleImageGeneratorFactory`: resolve o provider atual
- `OpenAiVehicleImageGenerator` e `GeminiVehicleImageGenerator` implementam a mesma interface
- A decisão do provider vem das feature flags do Unleash, e não de valores hardcoded

Recomendação prática para este repositório:

- `auto-image-generation-enabled` — liga/desliga a funcionalidade
- `auto-image-openai-enabled` — usa OpenAI quando ativo
- `auto-image-gemini-enabled` — usa Gemini quando ativo

Regra de resolução:

1. Se `auto-image-generation-enabled` estiver desligada → não gerar imagem
2. Se `auto-image-openai-enabled` estiver ligada → usa `OpenAiVehicleImageGenerator`
3. Senão, se `auto-image-gemini-enabled` estiver ligada → usa `GeminiVehicleImageGenerator`
4. Se nenhuma estiver ligada → fallback seguro: desativa a geração e regista aviso operacional

Importante: não colocar em feature flag:

- API keys
- endpoints
- timeouts
- nomes de bucket / storage
- credenciais de SMTP

Essas devem continuar em `appsettings` / configurações de infra. Feature flag serve para controlar o comportamento do produto em tempo real e permitir rollout gradual.

### 5.7 Templates Dinâmicos

Os emails de lembrete devem usar templates dinâmicos para permitir i18n, personalização e fácil manutenção.

- Onde guardar:
    - EmbeddedResource (rápido e consistente com `Zelo.Modules.Identity`): os ficheiros HTML ficam embutidos nos assemblies e são carregados por `EmailTemplateLoader`.
    - Blob storage / DB (opcional): permite editar templates sem deploy; útil para product owners.

- Motor de render:
    - Simples: `EmailTemplateLoader.Load(...)` + `string.Replace` para placeholders controlados. Bom para MVP.
    - Recomendo `Scriban` para templates seguros, leves e com lógica básica (`if`, `for`, formatação de datas).
    - `RazorLight` só se precisar de Razor completo (mais peso).

- Placeholders recomendados (todos substituídos/escapeados conforme necessário):
    - `{{Preheader}}`, `{{Heading}}`, `{{BodyHtml}}`
    - `{{Plate}}`, `{{VehicleBrand}}`, `{{VehicleModel}}`
    - `{{DueOn}}` (formatado por locale), `{{DaysUntilDue}}`
    - `{{HouseholdName}}`, `{{ActionUrl}}`

- Boas práticas:
    - Escapar todo o texto inserido com `WebUtility.HtmlEncode`.
    - Permitir `BodyHtml` quando for HTML confiável (ou gerar o HTML a partir de dados simples).
    - Suportar templates por `locale` (`vehicle-reminder.pt.html`, `vehicle-reminder.en.html`).
    - Versão de templates (campo `TemplateVersion` ou metadata) para auditoria e rollback.

- Integração com o handler:
    - `ObligationReminderDueHandler` resolve o `TemplateId` (ex.: `vehicle-reminder`) e `locale`, carrega o template e o renderiza com os dados do evento, depois chama `INotificationEmailSender.SendAsync(to, subject, body)`.
    - `INotificationEmailSender` pode reutilizar a implementação SMTP usada em `Zelo.Modules.Identity` internamente, mas expor uma interface independente (`INotificationEmailSender`) registada em `CoreModule`.

- Exemplo mínimo (Scriban-like pseudocódigo):
    - Template `vehicle-reminder.pt.html` contém `{{ Heading }}` e `{{ BodyHtml }}` e `{{ DaysUntilDue }}`.
    - Render:
        - carregar template string
        - engine.Render(template, new { Plate = "AB-12-CD", DueOn = "2026-09-30", DaysUntilDue = 15 })

- Testes:
    - Unit: renderizar template com dados e verificar placeholders/escaping.
    - Integration: enviar para Mailhog (dev) e validar HTML

Adicionar esta secção torna explícito como os templates são geridos e integrados no fluxo de envio.
---

## 6. Frontend (UI)

### 6.1 Estrutura de Componentes

```
packages/ui/ (ou shell/)
├── components/
│   └── notifications/
│       ├── NotificationBell.tsx          (ícone com contador, no shell)
│       ├── NotificationCenter.tsx        (painel: lista de core.notification_logs)
│       └── NotificationPreferences.tsx   (daysWarning + email/push on-off)
```

Não é preciso nenhum formulário de "criar notificação" — a data já existe (é a data de
inspeção/seguro do veículo). O frontend só precisa de:

1. **Bell icon com contador** — GET periódico (polling, ex. 60s) a
   `/api/households/{id}/notifications` para saber quantas estão por reconhecer.
2. **Notification Center** — lista as notificações (título da obrigação + dias restantes) e
   permite marcar como lida (`POST .../acknowledge`).
3. **Preferências** — um simples formulário com `daysWarning` e toggles de email/push, no
   ecrã de definições do household.

---

## 7. Plano de Implementação Faseado

### **Fase 1: Base de dados + evento**
- [ ] Migração: `core.notification_logs` e `core.notification_preferences`
- [ ] Novo evento `ObligationReminderDue` em `Zelo.Contracts`
- [ ] Garantir que o Auto publica `ObligationScheduled`/`Updated` também para o seguro (hoje só a inspeção)

### **Fase 2: Produtor (Worker)**
- [ ] `ObligationReminderCheckService` (BackgroundService, registado só no `Zelo.Worker`)
- [ ] Query de obrigações pendentes dentro do prazo de aviso, por household

### **Fase 3: Consumidor / envio real**
- [ ] `ObligationReminderDueHandler` + `AddZeloEventHandler<...>` em `AddCoreConsumers`
- [ ] `INotificationEmailSender` (SMTP, independente do de Identity) + template de email
- [ ] Idempotência (não reenviar se já existe log) e teste da DLQ

### **Fase 4: API + Frontend**
- [ ] Endpoints de leitura/acknowledge/preferências em `Zelo.Modules.Core`
- [ ] `NotificationBell` + `NotificationCenter` no shell (polling)
- [ ] Ecrã de preferências (daysWarning, email/push on-off)

### **Fase 5: Refinamento**
- [ ] Testes automatizados (handler, background service, endpoints)
- [ ] Push notifications (V1.1, fora do MVP)
- [ ] Documentação + deploy

---

## 8. Configuração

### 8.1 appsettings.json

```json
{
  "NotificationSettings": {
    "EnableNotifications": true,
    "CheckIntervalMinutes": 60,
    "DefaultDaysWarning": 15,
    "MaxRetries": 3,
    "Channels": {
      "Email": {
        "Enabled": true,
        "TemplateId": "vehicle_notification"
      },
      "Push": {
        "Enabled": true,
        "Provider": "firebase"
      },
      "InApp": {
        "Enabled": true,
        "RetentionDays": 30
      }
    }
  }
}
```

### 8.2 Configuração do Worker

```csharp
// Zelo.Worker/Program.cs
builder.Services.AddScoped<NotificationScheduler>();

var app = builder.Build();

// Executar verificações periódicas
var scheduler = app.Services.GetRequiredService<NotificationScheduler>();
_ = PeriodicTimer(async () => 
{
    await scheduler.ProcessNotificationsAsync();
}, TimeSpan.FromHours(1));
```

### 8.3 Feature flags recomendadas

Para o projeto atual, as feature flags fazem sentido para ligar ou desligar comportamento do produto e para rotear entre fornecedores de geração de imagem.

Exemplo de flags do Unleash:

```text
auto-image-generation-enabled
auto-image-openai-enabled
auto-image-gemini-enabled
```

O bootstrap destas flags deve ser feito em `Zelo.MigrationRunner/UnleashBootstrap.cs`, seguindo o mesmo padrão das restantes flags do projeto (`auto-app-enabled`, `auto-mcp-enabled`, etc.).

Em termos de regras de arquitetura:

- a API não decide provedor; apenas o Worker resolve
- a flag controla comportamento, não credenciais
- a lógica de fallback fica no Worker, nunca no frontend
- a escolha do provider é feita exatamente no ponto onde a geração de imagem corre

---

## 9. Testes

### 9.1 Testes Unitários
- `NotificationService`: criar/atualizar/eliminar regras
- `NotificationScheduler`: lógica de verificação
- `VehicleNotificationRule.ShouldNotify()`: cálculos de data

### 9.2 Testes de Integração
- Criar regra e verificar BD
- Disparar scheduler e verificar eventos publicados
- Fluxo completo: regra → notificação → histórico

### 9.3 Testes E2E
- Criar regra via API
- Aguardar trigger
- Verificar notificação na UI

---

## 10. Considerações Especiais

### 10.1 Timeouts e Agendamento
- Use `BackgroundService` ou Hangfire se necessário escalabilidade
- Considerar fuso horário do utilizador ao calcular datas

### 10.2 Performance
- Indexar `VehicleNotificationRules` por `Enabled` e `TargetDate`
- Arquivar `NotificationLogs` antigas (> 90 dias)

### 10.3 Segurança
- Validar que utilizador só vê notificações dos seus veículos
- Auditoria de mudanças em regras

### 10.4 Escalabilidade
- Usar fila de mensagens (MassTransit) para entrega assíncrona
- Cache de preferências de notificação por utilizador

---

## 11. Prioridades de MVP

1. **V1 (MVP)**:
   - CRUD de regras de notificação
   - Agendador básico
   - Email e In-App apenas
   - Histórico simples

2. **V1.1**:
   - Push notifications
   - Preferências de canal
   - UI melhorada

3. **V2**:
   - Notificações recorrentes (anualmente para seguro)
   - Templates customizáveis
   - Analytics de engajamento

---

## 12. Riscos e Mitigação

| Risco | Impacto | Mitigação |
|-------|--------|-----------|
| Notificações perdidas | Alto | Implementar retry logic + DLQ |
| Performance degradada | Médio | Indexar BD, arquivar dados antigos |
| Email spam | Médio | Respeitar preferências, rate limiting |
| Timezone issues | Médio | Usar DateTime.UtcNow sempre |
| Escalabilidade | Médio | Usar MassTransit + background services |

---

## Próximos Passos

1. ✅ Revisar e aprovar este plano
2. ⬜ Iniciar Fase 1: Setup de BD e entidades
3. ⬜ Criar branch feature: `feature/notifications`
4. ⬜ Abrir PRs incrementais por fase
5. ⬜ Deploy progressivo com feature flags

