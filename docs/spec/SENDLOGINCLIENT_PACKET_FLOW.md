# sendLoginClient Packet Flow

Authoritative source: `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerLogin.cpp`.

## Confirmed Order Before Warp

For a normal client path with no old-version map workaround, no login flags, no weapons/classes/protected weapons, and no zlib-fix branch, the pre-warp order is:

```txt
sendProps(__sendLogin)
PLO_CLEARWEAPONS
player flags as PLO_FLAGSET
server flags as PLO_FLAGSET
PLO_NPCWEAPONDEL "Bomb"
PLO_NPCWEAPONDEL "Bow"
PLO_SERVERLISTCONNECTED / C++ symbol PLO_UNKNOWN190
STOP before warp(m_levelName, getX(), getY())
```

`sendProps(__sendLogin)` builds:

```txt
PLO_PLAYERPROPS + encoded property payload
```

`Player::sendPacket` appends `\n` to each packet unless already present.

## Implemented C# Boundary

`PostLoginWorldEntryBoundary.BeginClient` implements the source-confirmed packet wrappers and ordering above using caller-provided encoded player-property payloads and ordered flag lists.

It marks the session:

```txt
ReadyForWorldEntry -> ReadyForLevelWarp
```

`ReadyForLevelWarp` means the port has reached the exact point before the C++ `warp(...)` call.

## Confirmed Packet IDs

- `PLO_PLAYERPROPS = 9`
- `PLO_FLAGSET = 28`
- `PLO_NPCWEAPONDEL = 34`
- `PLO_SERVERLISTCONNECTED = 190`; C++ source calls this `PLO_UNKNOWN190`
- `PLO_CLEARWEAPONS = 194`

## Deferred Branches Before Warp

These are traced but not implemented yet:

- spar deviation recalculation mutates account/player state
- old client `CLVER_2_31`/`CLVER_1_411` map-file workaround through `msgPLI_WANTFILE`
- `flaghack_ip` mutation of `gr.ip`
- weapon list emission and default weapon conversion through `msgPLI_WEAPONADD`
- protected weapon auto-add
- class packet emission for `m_versionId >= CLVER_4_0211`
- zlib-fix NPC weapon for client versions 2.21 through 2.31

The C# boundary only emits packets whose bytes are directly confirmed and whose input data is supplied as encoded fixture data.
