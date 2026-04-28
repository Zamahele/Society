# Testing Guide

## Purpose

This suite protects the core workflows so regressions are caught before release.

## Project

- `SocietyApp.Tests` (xUnit)
- Uses EF Core InMemory to validate service-level business rules.

## Current Coverage Baseline

### Membership Service

1. Membership number generation sequence (`SOC-0001`, `SOC-0002`, ...).
2. Dependant limit enforcement (max 10).
3. Overdue-driven suspension for active memberships.

### Payment Service

1. Joining fee confirmation marks payment confirmed and activates pending membership.
2. Monthly payment month normalization to first day.
3. Overdue detection when old expected month is unpaid.
4. Non-overdue behavior within grace window.

### Claim Service

1. Eligibility failure reasons when status/waiting period rules fail.
2. Eligibility pass for active members beyond waiting period.
3. Claim submission defaults (status, payout amounts, certificate fields).
4. Payout lifecycle transitions (`PartiallyPaid` -> `FullyPaid`).

## Run Tests

From repository root:

```bash
dotnet test Society.sln
```

## CI Recommendation

Add this mandatory step to your pipeline for every PR:

```bash
dotnet restore Society.sln
dotnet build Society.sln --configuration Release --no-restore
dotnet test Society.sln --configuration Release --no-build
```

## Extension Plan

1. Add controller tests for authorization and redirect behavior.
2. Add integration tests for critical admin/member web flows.
3. Add regression test whenever a production bug is fixed.
