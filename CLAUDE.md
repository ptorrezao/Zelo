# Claude project instructions

## Mandatory ADR workflow

This repository is architecture-driven. Before proposing or implementing a change that affects architecture, contracts, integrations, persistence, workflows, notification delivery, or operational boundaries, you must review the ADRs in [docs/adr](docs/adr) first.

### Required behaviour

- Read the existing ADRs before making design decisions.
- If the change impacts an existing decision, update the relevant ADR or add a new one.
- If a change is architectural in nature and no ADR exists, create a new ADR in [docs/adr](docs/adr) before implementation is considered complete.
- Use the ADR naming pattern: `000X-short-title.md` and maintain the current numbering order.
- Keep decisions explicit: context, decision, rationale, consequences, and status.
- Preserve the project architecture: modular monolith, module boundaries, API host vs Worker host, messaging contracts, and migration discipline.

## Architectural constraints

- The API host is for endpoints and request processing only.
- Background jobs, event consumers, and scheduled work belong to the Worker host.
- Shared contracts must live in [backend/src/Zelo.Contracts](backend/src/Zelo.Contracts).
- Messaging abstractions must use [backend/src/Zelo.Messaging](backend/src/Zelo.Messaging) and not ad hoc integrations.
- Cross-module coupling is forbidden; modules may not reference each other directly.
- Database changes must be treated as migration-sensitive work and documented with migration impact.

## Notification and reminder work

For reminder, notification, deadline, or expiry features:

- Check the notification plan in [plans/notifications.md](plans/notifications.md).
- Confirm whether the logic belongs to an event producer, the Worker, or a scheduler.
- Validate whether the change touches templates, retries, DLQ, time-zone logic, retention policies, or privacy constraints.
- If the feature changes the architectural pattern, update or create the relevant ADR.

## Before completing work

Always ensure:

- the relevant ADR is referenced in the PR or issue,
- a GitHub issue exists for the work when the task is not already tracked,
- the design remains consistent with the project architecture,
- new or changed behavior is documented when it affects the system boundary,
- tests cover the changed behavior,
- any drift from existing ADRs is called out explicitly.

## Issue creation rule

- Before implementation, check whether a GitHub issue already exists for the task.
- If no issue exists, create one in GitHub before continuing with implementation.
- The issue must contain context, desired behavior, scope, acceptance criteria, and risk/constraint notes.
- If the work affects architecture, notifications, messaging, persistence, or host boundaries, reference the relevant ADR or note that a new ADR will be added.

## Default rule

When in doubt, prefer the repository's documented architecture and ADRs over ad hoc solutions.
