# sendLevel Dynamic Packet Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelBoardChange.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelBoardChange.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelChest.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelHorse.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelBaddy.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelBaddy.h`
- `external/gs2lib/include/IEnums.h`
- `external/gs2lib/include/CString.h`

## Dynamic Packet Order

In modern `Player::sendLevel`, after the static level-name/board/modtime/link/sign
slice, C++ sends the dynamic level packets only when `fromAdjacent == false`:

```cpp
sendPacket(CString() << pLevel->getBoardChangesPacket(l_time));
sendPacket(CString() << pLevel->getChestPacket(this));
sendPacket(CString() << pLevel->getHorsePacket());
sendPacket(CString() << pLevel->getBaddyPacket(m_versionId));
```

`Player::sendPacket` appends `\n` to non-empty packets that do not already end
with `\n`.

## Board Changes

`Level::getBoardChangesPacket(time)` always starts with:

```txt
PLO_LEVELBOARD as GCHAR
```

Then it appends each `LevelBoardChange::getBoardStr()` whose
`change.getModTime() >= time`. The comparison is inclusive.

`LevelBoardChange::getBoardStr()` writes:

```txt
GCHAR x
GCHAR y
GCHAR width
GCHAR height
raw newTiles bytes
```

Because the packet starts with `PLO_LEVELBOARD`, an empty board-change list still
queues `[GCHAR PLO_LEVELBOARD, "\n"]` when `fromAdjacent == false`.

## Chests

For every level chest, `Level::getChestPacket(player)` computes:

```cpp
bool hasChest = pPlayer->hasChest(getChestStr(chest.get()));
```

Then it writes:

```txt
GCHAR PLO_LEVELCHEST
GCHAR hasChest ? 1 : 0
GCHAR chest.x
GCHAR chest.y
if !hasChest:
  GCHAR chest.itemIndex
  GCHAR chest.signIndex
"\n"
```

No chest entries means an empty packet; `Player::sendPacket` drops it.

## Horses

For every horse, `Level::getHorsePacket()` writes:

```txt
GCHAR PLO_HORSEADD
horse.getHorseStr()
"\n"
```

`LevelHorse::getHorseStr()` is cached after first construction and writes:

```txt
raw char(x * 2)
GCHAR(y * 2)
GCHAR((bushes << 2) | (dir & 0x03))
raw image bytes
```

The C# boundary currently accepts the already serialized `horse.getHorseStr()`
bytes because production horse runtime behavior is not implemented yet.

## Baddies

For every non-null baddy, `Level::getBaddyPacket(clientVersion)` writes:

```txt
GCHAR PLO_BADDYPROPS
GCHAR baddy.id
baddy.getProps(clientVersion)
"\n"
```

`LevelBaddy::getProps` iterates property IDs `1` through `BDPROP_COUNT - 1` and
appends each property ID as `GCHAR` followed by `getProp(id, clientVersion)`.
The C# boundary currently accepts already serialized baddy prop bytes because
full baddy runtime state and client-version-specific image behavior remain a
separate gameplay milestone.

Confirmed baddy property IDs:

```txt
BDPROP_ID = 0
BDPROP_X = 1
BDPROP_Y = 2
BDPROP_TYPE = 3
BDPROP_POWERIMAGE = 4
BDPROP_MODE = 5
BDPROP_ANI = 6
BDPROP_DIR = 7
BDPROP_VERSESIGHT = 8
BDPROP_VERSEHURT = 9
BDPROP_VERSEATTACK = 10
BDPROP_COUNT = 11
```

## Post-Dynamic Continuation

After dynamic packets, C++ sends:

1. `PLO_LEVELNAME + mapName` if the player has a GMAP context.
2. `PLO_GHOSTICON + GCHAR(0)` unconditionally.
3. `PLO_ISLEADER` only when `(!fromAdjacent || map context exists)` and the
   player is level leader or the level is singleplayer.
4. `PLO_NEWWORLDTIME + GINT4(server.getNWTime())` unconditionally.
5. `PLO_SETACTIVELEVEL + activeName` and `pLevel->getNpcsPacket(...)` only when
   `!fromAdjacent || map context exists`; `activeName` is the GMAP name for GMAP
   context, otherwise the level name.

The current C# implementation models this continuation with a snapshot payload
and accepts `NpcsPacket` as already serialized bytes. It stops before nearby
player prop forwarding.

## C# Status

Implemented:

- `LevelBoardChangePayload`
- `LevelChestPayload`
- `LevelHorsePayload`
- `LevelBaddyPayload`
- `LevelRuntimeContinuationPayload`
- packet ordering in `SendLevelBoundary.BeginModern`
- session stop states `DynamicLevelPayloadSent` and `LevelRuntimePacketsSent`

Not implemented:

- production board-change mutation/respawn behavior
- chest opening/runtime persistence
- horse runtime state
- baddy AI/combat/drop behavior
- production NPC serialization
- nearby player prop forwarding
- old `sendLevel141`
