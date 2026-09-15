# ADR-007: Timezone and Date Handling for Reminders

**Estado:** aceite
**Data:** 2026-09-15

## Contexto

Reminders are sensitive to timezone and locale. `core.obligations` stores `DueOn` as a `date`.
Without a TZ strategy, users may receive reminders at wrong local dates.

## Decisão

Store obligation `DueOn` as `date` and record `HouseholdTimezone` (IANA tz) for household metadata.
When rendering and computing `DaysUntilDue`, compute relative to the household's local date (convert UtcNow
to household timezone and take date component).

## Consequences

- Correct local behaviour for reminders across timezones.
- Requires storing timezone metadata and converting times at runtime.

## Alternatives

- Keep everything UTC and accept potential off-by-one local date issues.
- Store `DueOn` as timestamp with timezone — less aligned with domain concept of `date`.
