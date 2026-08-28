# Stack local (docker compose)

```bash
docker compose -f infra/compose/docker-compose.yml up -d
```

Nome do projeto Compose: `zelo` (containers `zelo-*`, volumes `zelo_*`) —
definido em `name: zelo` no topo do `docker-compose.yml`.

Todas as credenciais abaixo são **só para dev local**. Nunca usar em
produção — lá vêm de secrets (ver conversa sobre gestão de secrets).

## Postgres (`db`)

| | |
|---|---|
| Host (fora do compose) | `localhost:5433` |
| Host (dentro do compose) | `db:5432` |
| User / Password | `zelo` / `zelo` |
| Database | `zelo` |

## LavinMQ (`lavinmq`)

| | |
|---|---|
| AMQP | `localhost:5672` |
| UI de management | http://localhost:15672 |
| User / Password | `guest` / `guest` |

A imagem não suporta definir a password do utilizador por defeito via env
var sem um hash pré-calculado — fica-se com o `guest` nativo, que já vem
acessível fora de loopback nesta imagem (ver comentário no compose file).

## Garage (`garage`) — armazenamento S3-compatible

| | |
|---|---|
| API S3 | http://localhost:3900 |
| Admin API | http://localhost:3903 |
| Bucket | `zelo-documents` |
| Access Key / Secret Key (usados pela Api/Worker) | `GK150252b5f2fe57b6e3250b27` / `bd257b59116ab67336fbe92145ffc0f7e4149d599f82070281dedd036af4579c` |
| Admin token (`/etc/garage.toml`, usado só pelo migrator) | `a7cd1d89f750c10d8693ef94d6877b2c` |

Layout do cluster (1 nó), bucket e chave são criados automaticamente pelo
`migrator` no arranque (`GarageBootstrap.cs`, via `Storage__Admin*` no
compose) — nada manual a fazer. A chave é importada com um valor fixo
(`/v1/key/import`) em vez de gerada, precisamente para poder ficar
hardcoded no compose como as outras credenciais de dev.

## Mailhog (`mailhog`) — captura de emails

| | |
|---|---|
| SMTP (usado pela Api) | `mailhog:1025` |
| UI web (ver emails capturados) | http://localhost:8025 |

Sem autenticação. Todos os emails da app (confirmação de conta, reset de
password) ficam aqui — nunca são enviados de verdade.

## Jaeger (`jaeger`) — tracing

| | |
|---|---|
| UI | http://localhost:16686 |
| OTLP gRPC / HTTP (usado pela Api/Worker) | `jaeger:4317` / `jaeger:4318` |

Sem autenticação.

## Unleash (`unleash`) — feature flags

| | |
|---|---|
| UI | http://localhost:4242 |
| Login da UI | `admin` / `unleash4all` (default da imagem, não definido por nós) |
| Admin API token (usado pela Api/Worker) | `*:*.unleash-insecure-api-token` |

O token de admin tem de ter o formato `<projeto>:<ambiente>.<segredo>` com
`*:*` (todos os projetos/ambientes) — a imagem rejeita tokens de admin
com escopo a um único projeto.

## Base de dados interna do Unleash (`unleash-db`)

| | |
|---|---|
| User / Password | `unleash` / `unleash` |
| Database | `unleash` |

Postgres à parte da BD principal (`db`) — o Unleash gere o próprio schema.
