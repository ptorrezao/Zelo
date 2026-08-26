# Zelo

Gestão de ativos pessoais e das obrigações que trazem consigo:
manutenção, seguros, inspeções, garantias, inventário.

## Arquitetura

Monólito modular **extraível**: os módulos são bibliotecas com fronteiras
verificadas por testes de arquitetura, carregadas por hosts distintos.
Não são microserviços — mas podem vir a ser, sem reescrita.

- `backend/src/Hosts/` — os únicos executáveis (Api, Worker, MigrationRunner)
- `backend/src/Modules/` — bibliotecas, nunca se referenciam entre si
- `backend/src/Zelo.Contracts/` — a única superfície pública entre módulos
- `backend/src/Zelo.Messaging/` — bus atrás de interface própria (ver ADR-002)
- `frontend/apps/` — microfrontends com deploy independente

## Primeiros passos

```bash
cd backend && dotnet restore && dotnet build
cd ../frontend && pnpm install
```

Antes de arrancar, ver `docs/adr/` — as decisões estruturais estão lá
com o respetivo raciocínio.
