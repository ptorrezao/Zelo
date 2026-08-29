# Deploy via Dokploy

Cada environment Dokploy (`development`, `production`, dentro do mesmo
projecto) tem a mesma estrutura, com valores diferentes:

- **2 Databases nativas** (Postgres) — `db` (app principal) e `unleash-db`
- **1 Compose de infra** (`infra/compose/docker-compose.dokploy.yml`) — LavinMQ, Garage, Jaeger, Unleash
- **6 Applications**, uma por app — `api`, `worker`, `migrator`, `shell`, `auto`, `inventory`

Não há um "compose gigante" com tudo lá dentro — cada app de código é a
sua própria Application na Dokploy (build, logs, deploy e domínio
independentes), o que também é o que torna possível o GitHub Actions
fazer deploy só do que fizer sentido de cada vez.

## Rede

Um Compose isolado **não entra automaticamente** na rede partilhada onde
vivem as Databases nativas e as Applications — é preciso juntar-se à
`dokploy-network` explicitamente. Já está feito no
`docker-compose.dokploy.yml` (`networks: [dokploy-network]` em cada
serviço, com `external: true` porque a rede já existe, criada pela
própria Dokploy). Sem isto, dá exactamente este erro ao arrancar o
Unleash: `getaddrinfo EAI_AGAIN <host-do-unleash-db>`.

Em cada Application (`api`, `worker`, `migrator`), **Advanced → Network**,
aponta também para **`dokploy-network`** (o mesmo nome fixo) — não é a
"rede do Compose", é a rede partilhada da própria Dokploy. Se o hostname
simples (ex. `garage`) não resolver depois de fazeres deploy, tenta o
prefixo `tasks.` (ex. `tasks.garage`) — workaround documentado da própria
Dokploy para quando o DNS da mesh do Swarm falha.

## Databases nativas

Cria em **Project → Environment → Databases → Create Database → Postgres**,
uma vez por environment:

| Nome sugerido | Usada por |
|---|---|
| `db` | `api`, `worker`, `migrator` (`ConnectionStrings__Zelo`) |
| `unleash-db` | `unleash`, dentro do Compose de infra (`UNLEASH_DATABASE_URL`) |

Depois de criada, a Dokploy mostra o **Internal Host** de cada uma (algo
tipo `db-xxxx:5432`) — é esse valor que entra nas variáveis abaixo, não
`db:5432`.

## Compose de infra — Environment Variables

No painel do Compose (`docker-compose.dokploy.yml`), **Environment
Variables**:

| Variável | Valor |
|---|---|
| `UNLEASH_DATABASE_URL` | `postgres://unleash:<password>@<internal host do unleash-db>/unleash` |
| `UNLEASH_API_TOKEN` | token de admin, formato `*:*.<segredo>` (ver README local) |

`GARAGE_ADMIN_TOKEN` não é variável — está fixo dentro do compose e do
`garage.toml`, ver comentário no ficheiro.

## As 6 Applications — configuração comum

**Não builda a partir do código** (evita gastar CPU/RAM da VPS) — o
GitHub Actions compila e publica a imagem no GHCR
(`ghcr.io/ptorrezao/zelo-<app>:<branch>`), a Dokploy só faz `pull`.

Em cada Application, aba de Source, escolhe **Docker** (não "Github") e
preenche:

| Campo | Valor |
|---|---|
| Docker Image | `ghcr.io/ptorrezao/zelo-<app>:develop` (produção usa `:main`) — a tag é o nome do branch |

Substitui `<app>` por `api`, `worker`, `migrator`, `shell`, `auto` ou
`inventory`. O repo é público, por isso as imagens no GHCR saem públicas
por omissão — a Dokploy não precisa de credenciais de registry para o
`pull`. Não precisas de configurar Build Path, Dockerfile Path, Docker
Context Path nem Watch Paths — nada disso se aplica ao source "Docker".

`migrator`: **Advanced → Swarm Settings → Mode = Replicated Job** (não
"Replicated") — corre uma vez, termina, e não entra em crash-loop como
aconteceria com um serviço normal.

### Environment Variables por Application

**`api` e `worker`** (iguais nas duas):

| Variável | Valor |
|---|---|
| `ConnectionStrings__Zelo` | `Host=<internal host do db>;Port=5432;Database=zelo;Username=zelo;Password=<password>` |
| `Messaging__Host` / `Port` / `Username` / `Password` / `VirtualHost` | `<hostname do lavinmq no compose de infra>` / `5672` / `guest` / `guest` / `/` |
| `Storage__Endpoint` | URL pública do Garage (ver Domains abaixo) |
| `Storage__Region` | `garage` |
| `Storage__Bucket` | `zelo-documents` |
| `Storage__AccessKey` / `Storage__SecretKey` | livremente escolhidas — o `migrator` regista-as no Garage no arranque |
| `Otel__Endpoint` | `http://<hostname do jaeger>:4317` |
| `FeatureFlags__Url` | `http://<hostname do unleash>:4242` |
| `FeatureFlags__ApiToken` | mesmo valor de `UNLEASH_API_TOKEN` do compose de infra |

`Email__SmtpHost` / `Email__SmtpPort` só vai na **`api`** (só o módulo
Identity, que corre lá, envia emails) — relay SMTP real em produção.

**`migrator`**: mesmas `ConnectionStrings__Zelo`, `FeatureFlags__*`, mais:

| Variável | Valor |
|---|---|
| `Storage__AdminUrl` | `http://<hostname do garage>:3903` |
| `Storage__AdminToken` | `a7cd1d89f750c10d8693ef94d6877b2c` (fixo, ver compose de infra) |
| `Storage__Bucket`, `Storage__AccessKey`, `Storage__SecretKey` | iguais às da `api`/`worker` |

**`shell`, `auto`, `inventory`** — `NUXT_PUBLIC_API_BASE` e os 3
`NUXT_PUBLIC_ZELO_*` são iguais nas três (mesmo host, path-based routing
— ver Domains abaixo); só `NUXT_APP_BASE_URL` muda por app:

| Variável | Valor |
|---|---|
| `NUXT_PUBLIC_API_BASE` | URL pública da `api` (subdomínio próprio, fora deste esquema) |
| `NUXT_PUBLIC_ZELO_SHELL` | `https://<host>` (ex: `https://zelo-dev.hugetower.cloud`) |
| `NUXT_PUBLIC_ZELO_AUTO` | `https://<host>/auto` |
| `NUXT_PUBLIC_ZELO_INVENTORY` | `https://<host>/inventory` |
| `NUXT_APP_BASE_URL` | `shell`: não definir (fica `/`) · `auto`: `/auto/` · `inventory`: `/inventory/` |

`NUXT_PUBLIC_COOKIE_DOMAIN` **não é usado** neste esquema — as 3 apps
partilham host, por isso o cookie de sessão já funciona sem precisar de
domain explícito (fica preso ao host por omissão do browser).

## Domains a configurar (por Application/serviço)

**`shell`/`auto`/`inventory` partilham um único host, um Path cada** —
routing por Path é nativo da Dokploy (Traefik por baixo), não precisa de
gateway próprio. **Strip Path desligado nas três** — cada app já sabe
lidar com o seu próprio prefixo via `NUXT_APP_BASE_URL` (ver secção
anterior); se a Dokploy tirasse o prefixo antes de encaminhar, a app
deixava de saber onde está montada e os assets/rotas partiam.

| Serviço | Porta interna | Path | Strip Path |
|---|---|---|---|
| `shell` | 3000 | `/` | desligado |
| `auto` | 3000 | `/auto` | desligado |
| `inventory` | 3000 | `/inventory` | desligado |

Host (igual nas três, por environment): `zelo-dev.hugetower.cloud` em
`development`, `zelo.hugetower.cloud` em `production`.

**`api` e `garage` ficam fora deste esquema**, cada um no seu próprio
subdomínio — meter a API S3 do Garage atrás de um path prefix arrisca
partir a assinatura SigV4 dos URLs pré-assinados, e a `api` já usa `/api`
internamente nas suas rotas, não precisa de mais nenhum prefixo:

| Serviço | Porta interna | Domain — `development` | Domain — `production` |
|---|---|---|---|
| `api` | 8080 | `api.zelo-dev.hugetower.cloud` | `api.zelo.hugetower.cloud` |
| `garage` (Compose de infra) | 3900 | `storage.zelo-dev.hugetower.cloud` | `storage.zelo.hugetower.cloud` — **obrigatório**: uploads fazem PUT direto do browser para uma URL pré-assinada; sem isto, falham |

Como a `api` fica numa origem diferente das 3 apps Nuxt, o CORS continua
necessário — `Cors__AllowedOrigins__0` na `api` aponta para
`https://zelo-dev.hugetower.cloud` (ou `https://zelo.hugetower.cloud` em
produção); como as 3 apps agora partilham essa origem, basta uma entrada.

`worker` e `migrator` não servem HTTP, sem Domain. Do Compose de infra,
só `garage:3900` precisa de Domain — `lavinmq`, `jaeger` e `unleash`
ficam internos (dá um Domain ao `unleash` se quiseres a UI acessível).

## Antes do primeiro deploy

1. Cria as 2 Databases, depois o Compose de infra, depois as 6
   Applications, nesta ordem — precisas dos Internal Hosts das Databases
   e dos hostnames do compose de infra para preencher as env vars acima.
2. Aponta o DNS de cada domínio para o IP da VPS antes de configurar os
   Domains na Dokploy (pede o certificado Let's Encrypt na hora).
3. `Email__SmtpHost`/`Port` de produção têm de ser um relay real — não há
   Mailhog neste setup.
