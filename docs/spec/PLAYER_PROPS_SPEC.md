# Player Property Serialization Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerProps.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Account.h`
- `external/gs2lib/include/CString.h`
- `external/gs2lib/include/IEnums.h`

## sendProps

`Player::sendProps` constructs a property payload, then sends:

```cpp
sendPacket(CString() >> (char)PLO_PLAYERPROPS << propPacket);
```

Confirmed behavior:

1. If the player is a client older than `CLVER_2_1`, `pCount` is forced to `37`.
2. Iterate property IDs from `0` to `pCount - 1`.
3. If `pProps[i]` is true, append `GCHAR i` and then `getProp(i)`.
4. Wrap the payload with `PLO_PLAYERPROPS`.
5. `Player::sendPacket` appends `\n` if the packet does not already end in newline.

## CString Operators Used By getProp

Recovered `CString.h` confirms:

- `operator>>(char)` -> `writeGChar`
- `operator>>(short)` -> `writeGShort`
- `operator>>(int)` -> `writeGInt`
- `operator>>(long long)` -> `writeGInt5`
- `operator<<(char)` -> raw byte
- `operator<<(short)` -> raw big-endian short
- `operator<<(int)` -> raw big-endian int
- `operator<<(CString)` -> raw string bytes

This is important: many player properties use Graal-packed integers, not raw big-endian integers.

## __sendLogin

`__sendLogin` is an 83-entry boolean table in `Player.cpp`. True entries are sent in ascending property ID order.

Confirmed first entries:

```txt
0 false
1 true
2 true
3 true
4 true
5 true
6 true
7 false
8 true
9 true
10 true
11 true
12 false
13 true
14 false
15 false
16 false
17 true
18 true
19 false
20 false
21 true
22 true
23 true
...
```

The full table is documented from C++ but not yet fully implemented in C# because many fields depend on account/default loading, level/map state, client version, or gameplay state.

## Confirmed Serializer Subset

The C# `PlayerPropertySerializer` implements source-confirmed encodings for a safe subset of `getProp`.

Examples:

- `PLPROP_MAXPOWER`: `GCHAR m_maxHitpoints`
- `PLPROP_CURPOWER`: `GCHAR (hitpoints * 2)`
- `PLPROP_RUPEESCOUNT`: `GINT gralats`
- `PLPROP_SWORDPOWER`: `GCHAR (swordPower + 30)`, `GCHAR swordImage.length`, raw sword image bytes
- `PLPROP_CURLEVEL`: `GCHAR levelName.length`, raw level name bytes for the non-GMAP/non-singleplayer fixture path
- `PLPROP_IPADDR`: `GINT5 accountIp`
- `PLPROP_ACCOUNTNAME`: `GCHAR accountName.length`, raw account name bytes

The serializer takes explicit property IDs and sorts them ascending to match `sendProps`.

## Side Effects

`getProp` itself is serialization only for the confirmed subset. Side effects are outside this milestone.

`setProps` can mutate gameplay/account state and forward packets; it was traced only to understand serialization conventions and is not implemented.
