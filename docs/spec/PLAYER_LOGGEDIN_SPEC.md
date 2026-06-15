# Server::playerLoggedIn Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/ServerList.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerLogin.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerProps.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Account.h`
- `external/gs2lib/include/IEnums.h`

## Scope

This milestone starts at the previous stop point:

```cpp
m_server->playerLoggedIn(shared_from_this());
```

It then traces the beginning of `Player::sendLoginClient()` and stops before:

```cpp
warp(m_levelName, getX(), getY())
```

That `warp` call starts level loading, map/runtime state, and `CFileQueue::sendCompress(true)`, so it remains outside this milestone.

## Server::playerLoggedIn

Confirmed C++ behavior:

```cpp
getServerList().addPlayer(player);
```

When `V8NPCSERVER` is enabled, the server also iterates database NPCs and queues:

```txt
npc.playerlogin
```

for each NPC. Scripting execution is not implemented in C#.

## ServerList::addPlayer

`ServerList::addPlayer` builds `SVO_PLYRADD`:

```txt
GCHAR SVO_PLYRADD
raw short player id
GCHAR player type bitfield
GCHAR PLPROP_ACCOUNTNAME + encoded account-name prop
GCHAR PLPROP_NICKNAME + encoded nickname prop
GCHAR PLPROP_CURLEVEL + encoded current-level prop
GCHAR PLPROP_X + encoded x prop
GCHAR PLPROP_Y + encoded y prop
GCHAR PLPROP_ALIGNMENT + encoded alignment prop
GCHAR PLPROP_IPADDR + encoded ip-address prop
```

Confirmed IDs:

- `SVO_PLYRADD = 14`
- `PLPROP_ACCOUNTNAME = 34`
- `PLPROP_NICKNAME = 0`
- `PLPROP_CURLEVEL = 20`
- `PLPROP_X = 15`
- `PLPROP_Y = 16`
- `PLPROP_ALIGNMENT = 32`
- `PLPROP_IPADDR = 30`

The C# `PostLoginWorldEntryBoundary.BuildServerListAddPlayerPacket` accepts already encoded property payloads. It does not invent account/player defaults.

## Client Visibility

`Server::playerLoggedIn` itself does not send a packet directly to the game client. Its confirmed non-scripting side effect is the list-server add-player packet above.

The next client-visible packets come from `Player::sendLoginClient`.
