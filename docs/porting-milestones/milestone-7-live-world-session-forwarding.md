# Live World Session Forwarding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement source-confirmed live player registry, level membership, visibility selection, and packet forwarding across sessions.
**Architecture:** `GServ.Game` manages world/session state; `GServ.Network` exposes session sinks; `GServ.Protocol` builds exact packet payloads.
**Tech Stack:** C#/.NET, xUnit multi-session tests, C++ server/player/level sources.

---

## Source Of Truth

- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/PlayerProps.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Level.cpp`
- Existing docs: `docs/spec/PLAYER_LOGGEDIN_SPEC.md`, `docs/spec/PLAYER_VISIBILITY_SYNC_SPEC.md`, `docs/spec/LEVEL_RUNTIME_OWNERSHIP_SPEC.md`.

## Required Work

- [ ] Re-trace `Server::playerLoggedIn`, player ID generation, duplicate session removal, level player list ordering, and deletion cleanup.
- [ ] Update `docs/spec/LIVE_WORLD_SESSION_FORWARDING_SPEC.md`.
- [ ] Add tests for player ID allocation, same-level membership order, removal behavior, area visibility selection, and forwarding packet order.
- [ ] Implement live multi-session forwarding only for confirmed packet types.
- [ ] Add guards for gameplay packet types that are not yet ported.
- [ ] Ensure dev-only single-client behavior remains opt-in.
- [ ] Run `dotnet build GServharp.sln`.
- [ ] Run `dotnet test GServharp.sln`.
- [ ] Confirm `git status --short ai_resources` is empty.
- [ ] Commit with message `Implement live world session forwarding`.

## Compatibility Constraints

- Preserve C++ player-list ordering and duplicate handling.
- Do not invent visibility radius or area math.
- Do not forward gameplay packets until their payloads are confirmed.

## Definition Of Done

- Multiple connected sessions can see and receive source-confirmed player property updates in C++ order.
