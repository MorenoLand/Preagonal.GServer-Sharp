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
* Pre-world auth/server-list boundary exists.
* Player::sendLogin pre-world boundary exists.
* ReadyForLevelWarp boundary exists.
* AccountFileParser exists.
* AccountLoadService exists for source-confirmed account file resolution.
* Player property serialization has a source-confirmed login subset and `__sendLogin` table.
* GraalFileQueue passthrough behavior exists.
* WarpPackets has isolated confirmed builders.
* Tests are green.

Now continue through these next safe milestones:

# 1. Warp/setLevel packet flow

Trace and document the beginning of `warp(...)`, `setLevel(...)`, and related level-entry functions.

Stop before full level runtime, NPC runtime, scripting execution, or gameplay simulation.

Focus on:

* Exact function chain from `warp(m_levelName, getX(), getY())`
* How level name is normalized/validated
* How x/y are handled
* When PLO_LEVELNAME is sent
* When PLO_PLAYERWARP or PLO_PLAYERWARP2 is sent
* When PLO_WARPFAILED is sent
* Packet order during first level entry
* Any branch for GMAP vs single level
* Any branch for missing level
* Any branch for client version
* Where real runtime begins

Allowed:

* Implement source-confirmed packet flow/state machine up to the first runtime boundary.
* Add DTOs/interfaces for level lookup results.
* Add packet builders only when exact bytes are confirmed.
* Add tests and golden fixtures.

Not allowed:

* Do not implement full level runtime.
* Do not implement NPC logic.
* Do not execute scripts.
* Do not invent level defaults.
* Do not fake level file content behavior as production.

# 2. Level/resource lookup boundary

Trace how the C++ server finds and loads level/resource files needed during warp.

Focus on:

* FileSystem usage
* level path lookup
* map/gmap lookup
* level file extension behavior
* missing level behavior
* resource/file transfer trigger points
* which file packets are queued
* how CFileQueue is used for level/resource transfer

Allowed:

* Add interfaces for level/resource lookup.
* Add test-only in-memory providers.
* Implement source-confirmed lookup behavior only.
* Document unsupported runtime behavior.

# 3. First level-entry packets

Implement the first source-confirmed packets emitted during warp/world-entry.

Focus on:

* packet IDs
* packet body structure
* packet order
* newline/bundle/filequeue behavior
* player position encoding
* level name encoding
* any required player props around warp

Allowed:

* Add golden byte fixtures.
* Add tests for exact packet bytes and ordering.
* Integrate with existing ReadyForLevelWarp boundary if safe.

# 4. CFileQueue expansion for resource transfer

Expand GraalFileQueue only if warp/resource transfer proves exact behavior.

Focus on:

* file packet queuing
* rawdata packets
* PLO_FILE/PLO_RAWDATA behavior
* compression threshold
* encrypted flush if source-confirmed
* socket partial write behavior if needed

Do not approximate compression/encryption/websocket behavior.

# 5. Player props around warp

Expand only properties directly required by first world-entry packets.

Focus on:

* level name
* x/y
* animation/gani if used
* direction if used
* sprite/head/body/sword/shield/colors if used
* any prop sent immediately around warp

Add tests for exact bytes.

# 6. Docs, tests, report

Update docs:

```txt
docs/spec/WARP_WORLD_ENTRY_SPEC.md
docs/spec/LEVEL_RESOURCE_SPEC.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/PLAYER_PROPS_SPEC.md
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
