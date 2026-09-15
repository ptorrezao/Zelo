# Development checklist

Use this checklist for every change to ensure no work is forgotten and the change stays traceable.

## Required before code review

- [ ] I linked this work to an issue or ADR.
- [ ] I described the user impact and business reason.
- [ ] I listed which modules are affected.
- [ ] I checked whether the change needs a migration.
- [ ] I verified that any background job is registered only in the Worker host, not in the API host.
- [ ] I added or updated tests for the behavior.
- [ ] I updated the docs or ADRs when the design changed.
- [ ] I reviewed whether the change affects privacy, security, or retention.
- [ ] I noted any follow-up work or known limitations.

## For work touching notifications / reminders

- [ ] If it changes event flow, I checked the `Zelo.Messaging` contract.
- [ ] If it sends email, I checked template rendering and escaping.
- [ ] If it changes deadlines, I checked timezone and date logic.
- [ ] If it changes behavior in production, I documented retry / DLQ / idempotency considerations.

## Release gate

- [ ] ADR / issue link is present in the PR description.
- [ ] Migration review is complete when relevant.
- [ ] CI and required tests passed.
- [ ] The change is consistent with module boundaries and operational host rules.

## Why this matters

This project intentionally separates the API host from the Worker host. Without explicit checks, work can be forgotten, jobs can be registered in the wrong host, or design shifts can go undocumented. This checklist is the operational guardrail for that.
