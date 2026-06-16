# Level Write/Delete Mutation Blockers

Status: source-confirmed blocker note for Phase 5. This document intentionally
does not specify a C# implementation yet. It records the C++ entry points that
must be traced in a dedicated mutation milestone before enabling level writes,
deletes, or persistent board edits.

## Source Files

- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Level.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelBoardChange.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelBoardChange.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/scripting/v8/V8LevelImpl.cpp`

## Confirmed C++ Mutation Entry Points

`Player::msgPLI_BOARDMODIFY` parses:

```txt
PLI_BOARDMODIFY
GCHAR x
GCHAR y
GCHAR width
GCHAR height
remaining bytes = tile payload
```

It calls `level->alterBoard(tiles, x, y, width, height, this)`. If that returns
true, C++ broadcasts:

```txt
PLO_BOARDMODIFY + original packet bytes after the packet id
```

to the current level through `Server::sendPacketToOneLevel`.

`Player::msgPLI_REQUESTUPDATEBOARD` parses a level name, mod time, x, y, width,
and height, but the recovered C++ implementation only logs the request and
returns true. The TODO asks what should be returned.

`Level::alterBoard` is the central runtime board-change mutation. Confirmed
behavior includes:

- rejects x/y outside `0..63`
- rejects width/height less than `1`
- rejects changes whose rectangle exceeds the `64x64` board
- optionally rejects clientside push/pull block changes when
  `clientsidepushpull` is true and the payload contains the confirmed red/blue
  block piece pattern
- sends the original tile payload back only to the initiating player when that
  push/pull rejection path triggers
- erases existing `m_boardChanges` fully contained within the new rectangle
- checks `respawningTiles` against the original top-left tile
- stores old tile bytes only for respawning changes
- creates `LevelBoardChange` with `respawntime` when respawning, otherwise
  `-1`

`LevelBoardChange` stores the rectangle, new tile bytes, old tile bytes, a
timeout, and a `time(0)` mod time. `getBoardStr()` serializes:

```txt
GCHAR x
GCHAR y
GCHAR width
GCHAR height
raw new tile bytes
```

`swapTiles()` swaps the stored new/old tile payloads for respawn handling.

`Level::saveLevel(filename)` writes a `.nw` text file. Confirmed high-level
ordering is:

```txt
GLEVNW01
BOARD rows for every non-transparent tile chunk on every layer
LINK entries
SIGN blocks
CHEST entries
BADDY blocks
LEVELNPC NPC blocks
```

It performs case-insensitive existing-file lookup by actual filename, creates a
new path from `getDirByExtension(getExtension(actualFilename))` when missing,
adds that path to the filesystem index, then writes with `std::ofstream`.

`V8LevelImpl.cpp::Level_Function_SaveLevel` exposes script
`level.savelevel(levelname)` and delegates directly to `levelObject->saveLevel`.

## Why C# Mutation Remains Blocked

The C# port currently has read-only filesystem indexing and source-confirmed
static level packet builders. It must not enable production write/delete
mutation yet because these compatibility details still need dedicated byte and
state fixtures:

- exact `CString::formatBase64` behavior for every saved tile value
- exact transparent tile sentinel handling across layers
- exact C++ `getLayers().size()` behavior when layer ids are sparse
- exact `getDirByExtension` destination paths for new saved levels
- filesystem index mutation side effects after save
- `std::ofstream` newline behavior on Windows vs the target deployment
- full `respawningTiles` table and `doTimedEvents` swap/delete timing
- old tile capture and `LevelBoardChange` mod-time filtering through
  `getBoardChangesPacket` and `getBoardChangesPacket2`
- item drop side effects from `msgPLI_BOARDMODIFY`, including `rand()`,
  `bushitems`, `vasesdrop`, and `tiledroprate`
- script-visible save behavior and error/return behavior
- rights/path checks for any RC/NC/admin-triggered write or delete operation

Until those are traced and fixture-tested, the C# level runtime must remain
read-only except for already-confirmed in-memory packet/snapshot boundaries.

## Required Future Milestone

A future source-confirmed mutation milestone should:

1. Add C++ fixture harnesses for `Level::alterBoard`, `LevelBoardChange`, and
   `Level::saveLevel`.
2. Capture golden bytes for accepted board modify, push/pull rejection, board
   change packets, respawn swap packets, and saved `.nw` text.
3. Trace and document `respawningTiles`, `doTimedEvents`, item drop RNG, and
   save path selection.
4. Implement C# mutation only after the fixtures above exist.
5. Keep delete/write APIs unavailable or explicitly guarded until rights/path
   semantics are fully traced.
