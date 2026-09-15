# Repository agent instructions

Follow these repository rules for all implementation work.

## ADR compliance

- Review [docs/adr](docs/adr) before any architectural decision.
- If the change affects architecture, integration contracts, messaging, persistence, notifications, host boundaries, or operational behavior, update or add an ADR.
- ADRs are the source of truth for major decisions.
- Do not implement an architectural workaround that contradicts an accepted ADR.

## Architecture rules

- Keep the modular monolith boundary intact.
- API hosts only handle request/response flows.
- Worker hosts handle background jobs and consumers.
- Use [backend/src/Zelo.Contracts](backend/src/Zelo.Contracts) and [backend/src/Zelo.Messaging](backend/src/Zelo.Messaging) for shared boundaries.
- Keep modules independent and avoid direct cross-module references.

## Traceability

- Link work to issue or ADR in PR descriptions.
- Keep documentation aligned with design decisions.
- Record migration, infra, and test impact when relevant.
- If no GitHub issue exists for the task, create one before implementation proceeds.
- Every task must be traceable to either a GitHub issue or an ADR in the final PR.

## Issue creation rule

- Before starting implementation, confirm whether a GitHub issue already exists.
- If no issue exists for the requested work, create one in GitHub and link it from the branch/PR.
- The issue must include: summary, scope, acceptance criteria, risks, and whether an ADR is needed.
- For architecture or delivery-impacting changes, the issue should explicitly state the ADR reference or note that a new ADR will be created.

## For notifications/reminders

- Validate the event-driven flow and scheduler placement.
- Check template, retry, DLQ, timezone, and privacy implications.
- If the design changes, add or update the matching ADR.
