Read `AGENTS.md`, `COMPATIBILITY_RULES.md`, `SERVER_SPEC.md`, `PORTING_PLAN.md`, `KNOWN_BLOCKERS.md`, and all docs under `docs/`.

Continue the C#/.NET 1:1 port using only source-confirmed behavior.

Source of truth:

```txt
ai_resources/GServer-CPP-ORIGINAL/
external/gs2lib/
```

Do not modify anything inside `ai_resources/`.

Do not invent behavior.

If something is unclear, document it as unknown and continue with the next safe task.

Current status:

* Login/session boundary exists.
* Account loading boundary exists.
* ReadyForLevelRuntime exists.
* SendLevelBoundary exists for the modern static sendLevel slice.
* Implemented safe modern sendLevel packets include:

  * PLO_LEVELNAME
  * raw board/layers with PLO_RAWDATA
  * PLO_LEVELMODTIME
  * links/signs as already serialized payloads
* Current stop point is before dynamic level runtime data:

  * board changes
  * chests
  * horses
  * baddies
  * GMAP correction
  * ghost icon
  * world time
  * active level
  * NPC packets
  * player props/forwarding

Now continue through these next safe milestones:

# 1. Dynamic level packet builders

Trace and implement source-confirmed builders/DTOs for the next sendLevel dynamic packets.

Focus on:

* `Level::getBoardChangesPacket`
* `Level::getChestPacket`
* `Level::getHorsePacket`
* `Level::getBaddyPacket`
* Any packet IDs/opcodes involved
* Exact field order
* Exact encoding
* Empty-list behavior
* Packet ordering inside sendLevel
* Version/client-type branches
* Whether each packet is always sent or conditional

Allowed:

* Add isolated DTOs/snapshots for board changes, chests, horses, and baddies.
* Add packet builders with exact byte tests.
* Integrate into SendLevelBoundary only when packet order and branch conditions are confirmed.
* Add golden fixtures for empty and small non-empty cases where source-confirmed.

Not allowed:

* Do not implement runtime behavior of board changes, chest opening, horse movement, or baddy AI.
* Do not implement NPC logic.
* Do not execute scripts.
* Do not invent default values.
* Do not approximate field encoding.

# 2. Continue sendLevel packet order after dynamic data

Trace the next confirmed packets after board changes/chests/horses/baddies.

Focus on:

* GMAP correction
* PLO_GHOSTICON
* PLO_NEWWORLDTIME
* PLO_SETACTIVELEVEL
* any player props resent around level entry
* any file/resource transfer packets
* exact ordering relative to dynamic packets

Allowed:

* Implement only confirmed packet builders/ordering.
* Add session state if a new safe boundary is reached.
* Add tests for exact packet order.

# 3. Level/resource transfer and CFileQueue integration

If sendLevel uses file queue/resource transfer in this slice, expand `GraalFileQueue` only where byte-exact behavior is source-confirmed.

Focus on:

* rawdata/file queue packets
* PLO_FILE
* PLO_RAWDATA
* compression flags
* bundle behavior
* encryption during flush
* queue thresholds

Do not approximate compression/encryption/socket behavior.

# 4. Docs and tests

Create/update docs:

```txt
docs/spec/SENDLEVEL_DYNAMIC_PACKETS_SPEC.md
docs/spec/SENDLEVEL_SPEC.md
docs/spec/LEVEL_FILE_FORMAT_SPEC.md
docs/spec/LEVEL_RESOURCE_SPEC.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/GOLDEN_FIXTURES.md
docs/spec/KNOWN_BLOCKERS.md
```

Run:

```bash
dotnet build GServharp.sln
dotnet test GServharp.sln
```

At the end, report:

* What was completed
* Which C++/gs2lib files were used
* Which C# files/tests were added or modified
* Which docs were updated
* Which golden fixtures were added
* Which behavior is now source-confirmed
* Which behavior remains blocked
* Whether `ai_resources/` stayed untouched
* Build/test results
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done.
