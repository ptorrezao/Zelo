# Deploy via Dokploy (`docker-compose.dokploy.yml`)

Este compose é o usado pela Dokploy, tanto no environment `development`
como no `production` do projecto — não é o mesmo do desenvolvimento local
(`docker-compose.yml`). Cada environment Dokploy corre a *sua própria*
instância deste stack, com os seus próprios valores para as variáveis
abaixo.

## Environment Variables a definir na Dokploy (por environment)

Painel do environment (dev ou prod) → **Environment Variables**:

| Variável | O que é | Nota |
|---|---|---|
| `DB_PASSWORD` | password do Postgres principal (`db`) | escolhe uma por ambiente |
| `UNLEASH_DB_PASSWORD` | password do Postgres do Unleash (`unleash-db`) | |
| `UNLEASH_API_TOKEN` | token de admin do Unleash | formato `*:*.<segredo>` — ver README local para porquê |
| `GARAGE_ACCESS_KEY` / `GARAGE_SECRET_KEY` | chave S3 importada no Garage pelo migrator | livremente escolhidas, o migrator regista-as no arranque |
| `SMTP_HOST` / `SMTP_PORT` | servidor SMTP real para envio de emails | produção precisa de um relay real (não Mailhog) |
| `PUBLIC_API_URL` | URL pública da Zelo.Api | ex: `https://api.zelo.pt` |
| `PUBLIC_SHELL_URL` / `PUBLIC_AUTO_URL` / `PUBLIC_INVENTORY_URL` | URLs públicas de cada app Nuxt | ex: `https://app.zelo.pt`, `https://auto.zelo.pt`, `https://inventory.zelo.pt` |
| `STORAGE_PUBLIC_ENDPOINT` | URL pública do S3 do Garage | ver Domains abaixo — tem de ser um domínio próprio |

`GARAGE_ADMIN_TOKEN` e a password do LavinMQ **não** estão na lista — ficam
fixos no compose (`a7cd1d89f750c10d8693ef94d6877b2c` e `guest`/`guest`
respectivamente), porque o Garage exige que bata certo com
`admin_token` em `infra/docker/garage/garage.toml` (ficheiro estático) e a
imagem do LavinMQ não aceita mudar a password do `guest` sem um hash
pré-calculado. Nenhum dos dois fica exposto fora da rede interna do
compose (sem `ports`, sem Domain), por isso o risco é equivalente ao de
qualquer outra password interna da stack.

## Domains a configurar na Dokploy (por serviço, por environment)

Cada app/serviço público precisa de um Domain próprio na UI da Dokploy
(Project → Environment → serviço → Domains):

| Serviço | Porta interna | Domain sugerido |
|---|---|---|
| `shell` | 3000 | `app.zelo.pt` (ou `dev.app.zelo.pt`) |
| `auto` | 3000 | `auto.zelo.pt` |
| `inventory` | 3000 | `inventory.zelo.pt` |
| `api` | 8080 | `api.zelo.pt` |
| `garage` | 3900 | `storage.zelo.pt` — **obrigatório**: os uploads de documentos fazem PUT direto do browser para uma URL pré-assinada gerada pela Api; sem domínio público aqui, os uploads falham |

Os restantes serviços (`db`, `lavinmq`, `garage` porta 3903/3901, `jaeger`,
`unleash`, `unleash-db`) ficam só na rede interna — não precisam de Domain.
Se quiseres a UI do Unleash acessível, dá-lhe também um Domain (porta 4242).

## Antes do primeiro deploy

1. Confirma no `Dockerfile.backend`/`Dockerfile.frontend` que o `context`
   aponta para a raiz do repo (`../..` a partir de `infra/compose/`) — a
   Dokploy tem de saber isso ao apontar para este ficheiro compose.
2. Aponta o DNS de cada domínio acima para o IP da VPS antes de configurar
   os Domains na Dokploy (ela pede o certificado Let's Encrypt na hora).
3. `SMTP_HOST`/`SMTP_PORT` de produção têm de ser um relay real
   (Resend, Postmark, SES, etc.) — não há Mailhog neste compose.
