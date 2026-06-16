# Level Map Format Specification

Status: source-confirmed pure map parser implemented for BIGMAP and GMAP
metadata. Production `Server::loadMaps` filesystem wiring and level preloading
remain blocked.

## Source Files

- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Map.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Map.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `external/gs2lib/src/CString.cpp`

## Map Types

C++ `MapType` values:

```txt
BIGMAP = 0
GMAP   = 1
```

Group map status is not encoded inside the map file. `Server::loadMaps` creates
maps with `pGroupMap = true` only when loading from the `groupmaps` server
option. Normal `gmaps` and `maps` entries are not group maps.

## BIGMAP

`Map::loadBigMap` uses `FS_FILE` when folder config is enabled, otherwise the
global filesystem. It resolves the requested map name by exact
`fileSystem->find(pFileName)`, stores `m_mapName = pFileName`, and returns false
if the file cannot be found.

File parsing:

1. Load lines with `CString::loadToken(fileName)` using newline delimiter.
2. For each line, remove `\r`, trim whitespace, and skip empty lines.
3. Run `guntokenize()`.
4. Split on `\n` with `keepEmpty = true`.
5. Compute row width excluding trailing empty entries only.
6. Overall map width is the maximum row width.
7. Height is the number of non-empty parsed rows.
8. Store non-empty level names lowercased in both flattened level list and
   lookup dictionary.

Interior empty cells are preserved. Trailing empty cells only affect width
calculation.

## GMAP

`Map::loadGMap` uses `FS_LEVEL` when folder config is enabled, otherwise the
global filesystem. It resolves the requested GMAP by exact `find`, stores
`m_mapName = pFileName`, and returns false if missing.

Recognized directives:

- `WIDTH value`
- `HEIGHT value`
- `GENERATED value`, parsed but ignored
- `LEVELNAMES` through `LEVELNAMESEND`
- `MAPIMG value`
- `MINIMAPIMG value`
- `NOAUTOMAPPING`, clientside only
- `LOADFULLMAP`
- `LOADATSTART` through `LOADATSTARTEND`

Malformed directives with the wrong token count are ignored for fields that
check count.

`LEVELNAMES` behavior:

1. Allocate a flattened list of `width * height`.
2. Blank trimmed lines are skipped without advancing GMAP y.
3. Stop at `LEVELNAMESEND`.
4. Only rows where `gmapy < height` are parsed.
5. `guntokenizeI()` converts comma-separated entries to newline-separated
   entries.
6. `tokenize("\n")` is called with default `keepEmpty = false`, so blank middle
   entries are compressed rather than preserved.
7. Names are lowercased and stored by coordinate until `gmapx == width`.

`LOADFULLMAP` sets `m_loadFullMap = true`.

`LOADATSTART` sets `m_loadFullMap = false` and records lowercased preload level
names after `guntokenizeI().tokenize("\n")`. Empty tokens are not preserved.

## Lookup Behavior

`Map::isLevelOnMap(level, mx, my)` uses an exact key lookup against lowercased
stored level names. Callers lower-case level names before lookup in confirmed
paths such as `Level::findLevel` and `Server::loadMaps` remapping.

`Map::getLevelAt(mx, my)` returns the flattened entry at `mx + my * width` when
the coordinates are inside bounds; otherwise it returns an empty string.

## Preload Behavior

`Map::loadMapLevels` is called only for GMAPs from `Server::loadMapLevels`.

- If `LOADFULLMAP` was set, every non-empty flattened level is loaded via
  `m_server->getLevel(levelName)`.
- Else if `LOADATSTART` names exist, each recorded preload name is loaded via
  `m_server->getLevel(level)`.
- The C++ asserts each load result.

The C# parser exposes `LevelsToPreload()` to preserve this selection without
performing production level loading yet.

## C# Status

Implemented:

- `MapFileParser.ParseBigMap`
- `MapFileParser.ParseGMap`
- `MapFileSnapshot`
- `MapFileType`
- C++ `guntokenize` behavior needed by map files
- lowercase level storage and exact lookup
- BIGMAP interior-empty/trailing-empty behavior
- GMAP directive parsing, image fields, `LOADFULLMAP`, `LOADATSTART`
- group-map metadata flag
- tests in `MapFileParserTests`

Blocked:

- Production `Server::loadMaps` settings-driven filesystem loader
- Production `Server::loadMapLevels` integration and assert behavior
- Reattaching cached `RuntimeLevel` objects to parsed maps
- File mod-time tracking for parsed maps
- Any gameplay/world simulation dependent on map ownership
