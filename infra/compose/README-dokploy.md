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

**`shell`, `auto`, `inventory`** (iguais nas três):

| Variável | Valor |
|---|---|
| `NUXT_PUBLIC_API_BASE` | URL pública da `api` |
| `NUXT_PUBLIC_ZELO_SHELL` / `_AUTO` / `_INVENTORY` | URLs públicas de cada app |

## Domains a configurar (por Application/serviço)

**Importante — nomenclatura aninhada, não paralela.** `shell`/`auto`/
`inventory` partilham cookies de autenticação entre si dentro do mesmo
environment (`NUXT_PUBLIC_COOKIE_DOMAIN`, ver `useApiClient.ts`). Um
`domain` de cookie só consegue isolar por sufixo de DNS — subdomínios
"irmãos" tipo `appdev.hugetower.cloud` e `app.hugetower.cloud` **não**
isolam nada entre si nem de outros projectos no mesmo domínio raiz. Por
isso cada environment vive debaixo do seu próprio sub-domínio dedicado:

| Serviço | Porta interna | Domain — `development` | Domain — `production` |
|---|---|---|---|
| `shell` | 3000 | `shell.zelo-dev.hugetower.cloud` | `shell.zelo.hugetower.cloud` |
| `auto` | 3000 | `auto.zelo-dev.hugetower.cloud` | `auto.zelo.hugetower.cloud` |
| `inventory` | 3000 | `inventory.zelo-dev.hugetower.cloud` | `inventory.zelo.hugetower.cloud` |
| `api` | 8080 | `api.zelo-dev.hugetower.cloud` | `api.zelo.hugetower.cloud` |
| `garage` (Compose de infra) | 3900 | `storage.zelo-dev.hugetower.cloud` | `storage.zelo.hugetower.cloud` — **obrigatório**: uploads fazem PUT direto do browser para uma URL pré-assinada; sem isto, falham |

`NUXT_PUBLIC_COOKIE_DOMAIN` em `shell`/`auto`/`inventory`:
- `development` → `.zelo-dev.hugetower.cloud`
- `production` → `.zelo.hugetower.cloud`

`zelo-dev.hugetower.cloud` e `zelo.hugetower.cloud` são subdomínios
**irmãos** (nenhum é sufixo do outro) — por isso um cookie com domínio
`.zelo-dev.hugetower.cloud` nunca chega a `zelo.hugetower.cloud`, nem a
qualquer outro projecto teu no mesmo `hugetower.cloud`. Se em vez disso
usasses algo como `appdev.hugetower.cloud` (sem o ponto extra), o
`domain` do cookie teria de ser `.hugetower.cloud` para cobrir as 3 apps
— o que partilharia a sessão com literalmente tudo o resto nesse domínio.

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
