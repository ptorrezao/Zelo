# Contrato de módulo

Para um projeto ser um módulo Zelo tem de cumprir os quatro pontos abaixo.
Um módulo novo que os cumpra funciona sem alterar o núcleo.

## 1. Registo

Expõe até quatro métodos de extensão, e nada mais público — os dois
primeiros são obrigatórios, os outros dois só se o módulo tiver endpoints
HTTP ou persistência (na prática, sempre):

```csharp
public static IServiceCollection AddXptoModule(this IServiceCollection s, IConfiguration c);
public static IServiceCollection AddXptoConsumers(this IServiceCollection s);
public static IEndpointRouteBuilder MapXptoEndpoints(this IEndpointRouteBuilder app);
public static Task MigrateAsync(IServiceProvider provider, CancellationToken ct = default);
```

- `AddXptoModule` — chamado pelos dois hosts (`Api` e `Worker`): regista o
  `DbContext`, as regras e os serviços do módulo.
- `AddXptoConsumers` — chamado APENAS pelo host `Worker`, depois de
  `AddZeloMessaging`: regista os `IEventHandler<T>` do módulo via
  `AddZeloEventHandler<TEvent, THandler>(queueName)`.
- `MapXptoEndpoints` — chamado APENAS pela `Api`, depois de
  `app.Build()`: regista as rotas HTTP do módulo (`app.MapGroup("/api/xpto")`).
  Exemplo real: `IdentityEndpoints.MapIdentityEndpoints`,
  `CoreEndpoints.MapCoreEndpoints`, `AutoEndpoints.MapAutoEndpoints`.
- `MigrateAsync` — chamado APENAS pelo `MigrationRunner`: aplica as
  migrations do `DbContext` do módulo. Existe porque o `DbContext` é
  `internal` ao módulo — esta é a única porta de saída para o correr.

Tudo o resto no módulo é `internal`, com uma excepção opcional — ver
secção 5.

### 5. MCP (opcional)

Um módulo pode expor um quinto método de extensão para dar a agentes LLM
acesso às suas próprias tools, via [Model Context Protocol](https://modelcontextprotocol.io/):

```csharp
public static IEndpointRouteBuilder MapXptoMcpEndpoints(this IEndpointRouteBuilder app);
```

- Chamado APENAS pela `Api`, depois de `MapXptoEndpoints` — mapeia um
  grupo de rota próprio (`/mcp/xpto`), com a sua própria feature flag
  (`xpto-mcp-enabled`), atrás da mesma autenticação do resto do módulo.
- As tools em si (`[McpServerToolType]`) ficam `internal`, junto dos
  endpoint handlers — reutilizam a mesma lógica de negócio, nunca a
  duplicam. Exemplo real: `AutoMcpTools`, montado por
  `AutoModule.MapAutoMcpEndpoints`.
- Uma tool que recebe `householdId` como argumento explícito (o MCP não
  tem query string) tem de confirmar a membership e, quando o recurso não
  tem `HouseholdId` próprio (ex.: uma manutenção, que só tem `VehicleId`),
  confirmar também que esse recurso pertence a esse household — mesmo que
  o endpoint REST equivalente não o faça.
- Fica de fora qualquer capacidade que exija o agente manusear
  credenciais ou segredos de terceiros (ex.: o fluxo de import do Auto).

## 2. Manifesto

Declara os tipos de ativo que gere e o JSON Schema dos atributos de cada
um. O shell do frontend lê o manifesto para construir menus e formulários
— nenhum formulário é escrito à mão por tipo de ativo.

## 3. Eventos

Publica os eventos definidos em `Zelo.Contracts`, nunca tipos próprios:

- `AssetCreated` / `AssetArchived`
- `ObligationScheduled` / `ObligationUpdated` / `ObligationCompleted`

O módulo `Core` consome-os para manter a timeline agregada. Nunca chama
os módulos para a construir.

## 4. Persistência

`DbContext` próprio com `HasDefaultSchema("xpto")` e migrations dentro do
módulo. Sem `JOIN` para fora do seu schema, em circunstância alguma.
