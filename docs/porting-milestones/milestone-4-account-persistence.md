# Account Persistence Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port C++ account loading, saving, default account behavior, guest handling, and pre-world player data semantics.
**Architecture:** Keep file-format parsers in `GServ.Persistence`; expose typed account/player DTOs to `GServ.Game`; avoid gameplay side effects outside confirmed boundaries.
**Tech Stack:** C#/.NET, xUnit, account golden fixtures derived from C++ format.

---

## Source Of Truth

- `ai_resources/GServer-CPP-ORIGINAL/server/src/Account.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Account.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/PlayerProps.cpp`
- C++ filesystem/settings code used by account folders and default accounts.
- Existing docs: `docs/spec/ACCOUNT_LOADING_SPEC.md`, `docs/spec/PLAYER_SENDLOGIN_SPEC.md`, `docs/spec/PLAYER_PROPS_SPEC.md`.

## Required Work

- [ ] Re-trace every account file field, parse order, missing field behavior, defaults, save order, and side effects.
- [ ] Update `docs/spec/ACCOUNT_PERSISTENCE_SPEC.md` and `docs/spec/ACCOUNT_LOADING_SPEC.md`.
- [ ] Add test fixtures for `GRACC001` and any other confirmed account format markers.
- [ ] Add failing tests for parse, round-trip save, missing file, default account load, banned/staff/admin fields, and guest account behavior where confirmed.
- [ ] Implement account repository behavior only for source-confirmed filesystem semantics.
- [ ] Wire account DTOs into the existing pre-world login boundary without inventing gameplay defaults.
- [ ] Run `dotnet build GServharp.sln`.
- [ ] Run `dotnet test GServharp.sln`.
- [ ] Confirm `git status --short ai_resources` is empty.
- [ ] Commit with message `Implement account persistence boundary`.

## Compatibility Constraints

- Preserve text encoding, field names, field order, numeric parsing, and missing-value behavior.
- Do not migrate to a new schema unless it is hidden behind exact compatibility behavior.

## Definition Of Done

- Source-confirmed account files can be loaded and saved with compatibility tests.
- Login boundary uses real account data where confirmed.
