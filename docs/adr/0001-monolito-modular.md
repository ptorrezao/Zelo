# ADR-001: Monólito modular em vez de microserviços

**Estado:** aceite
**Contexto:** produto sem utilizadores, um programador, domínio com núcleo
partilhado (Ativo / Obrigação / Evento) usado por todos os módulos.

## Decisão

Monólito modular com fronteiras rígidas, carregado por hosts separados
(Api e Worker) que correm em contentores independentes.

## Porquê

Separar por módulo de negócio (Automóvel, Inventário) criaria um
*distributed monolith*: cada serviço precisaria do núcleo, gerando ou
chamadas síncronas em cadeia ou duplicação do modelo. As fronteiras que
justificam serviço próprio são de **capacidade técnica**, não de domínio.

## Critério de extração

Um módulo sai para serviço próprio quando cumprir pelo menos um:

- perfil de recursos incompatível com o resto (CPU-bound, longa duração)
- necessidade de escalar independentemente sob carga medida
- cadência de deploy que colide com a dos outros módulos

O OCR de documentos é o primeiro candidato e provavelmente o único
durante bastante tempo.

## Consequências

- fronteiras dependem de disciplina; mitigado com `Zelo.ArchitectureTests`
- o bus é in-process mas com a API do bus distribuído (ver ADR-002)
- sem `JOIN` entre schemas, mesmo estando na mesma base de dados
