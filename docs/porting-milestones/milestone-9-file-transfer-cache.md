# File Transfer Cache Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port client file/resource transfer, wanted-file handling, update packages, cache checks, and raw-data file payload behavior.
**Architecture:** Resource lookup in `GServ.Persistence`, packet/file queue building in `GServ.Protocol`, socket flush in `GServ.Network`.
**Tech Stack:** C#/.NET, xUnit golden fixtures, C++ file queue and player file-transfer sources.

---

## Source Of Truth

- `ai_resources/GServer-CPP-ORIGINAL/server/src/Player.cpp`
- C++ file transfer helpers under `server/src`.
- `external/gs2lib/src/CFileQueue.cpp`
- `external/gs2lib/include/CFileQueue.h`
- Existing docs: `docs/spec/CFILEQUEUE_FLUSH_SPEC.md`, `docs/spec/LEVEL_RESOURCE_SPEC.md`.

## Required Work

- [ ] Re-trace `PLI_WANTFILE`, `PLI_RAWDATA` file receive/send, package update behavior, and cache-modtime comparisons.
- [ ] Update `docs/spec/FILE_TRANSFER_CACHE_SPEC.md`.
- [ ] Add fixtures for small, medium, large, compressed, and blocked bzip2 resource transfers.
- [ ] Add tests for queue ordering, raw-data boundaries, file-not-found behavior, and client cache response packets.
- [ ] Implement confirmed file transfer packet builders and session state transitions.
- [ ] Keep upload/write paths blocked until exact C++ security and overwrite behavior is confirmed.
- [ ] Run `dotnet build GServharp.sln`.
- [ ] Run `dotnet test GServharp.sln`.
- [ ] Confirm `git status --short ai_resources` is empty.
- [ ] Commit with message `Implement file transfer cache boundary`.

## Compatibility Constraints

- Preserve `CFileQueue` thresholds, compression selection, and packet order.
- Do not invent MIME/type behavior or cache invalidation rules.

## Definition Of Done

- Confirmed resource requests produce exact C++-compatible bytes.
- Unknown transfer branches are blocked with tests and docs.
