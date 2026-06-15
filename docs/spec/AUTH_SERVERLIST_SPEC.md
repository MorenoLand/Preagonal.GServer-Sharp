# Auth And Server-List Spec

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerLogin.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/ServerList.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Account.cpp`
- `external/gs2lib/include/IEnums.h`
- `external/gs2lib/src/CFileQueue.cpp`

## Pre-Server-List Checks

After `msgPLI_LOGIN` parses type/version/account/password/identity, the C++ server checks:

1. Player count: if `playerList.size() >= maxplayers`, send `PLO_DISCMESSAGE "This server has reached its player limit."`.
2. IP ban: if `Server::isIpBanned(remoteIp)` and the account does not have `PLPERM_MODIFYSTAFFACCOUNT`, send `PLO_DISCMESSAGE "You have been banned from this server."`.
3. Allowed versions for clients only: exact version tokens or inclusive `start:end` ranges are compared through `getVersionID`. Rejection sends `PLO_DISCMESSAGE "Your client version is not allowed on this server.\rAllowed: {allowedVersionString}"`.
4. Server-list connection: if not connected, send `PLO_DISCMESSAGE "The login server is offline.  Try again later."`.

Only after these checks does C++ call `ServerList::sendLoginPacketForPlayer`.

## Server-List Auth Request

`ServerList::sendLoginPacketForPlayer` sends `SVO_VERIACC2`:

```txt
GCHAR SVO_VERIACC2
GCHAR account length
account bytes
GCHAR password length
password bytes
GSHORT player id
GCHAR player type bitfield
GSHORT identity length
identity bytes
```

The packet is queued through `ServerList::sendPacket`, which appends `\n` if missing. The current C# builder emits the packet body before queue newline/compression so tests can lock the field order and raw bytes.

## Server-List Auth Response

`ServerList::msgSVI_VERIACC2` reads:

```txt
GCHAR account length
account bytes
GSHORT player id
GCHAR player type
remaining bytes as message
```

The response overwrites the local player account name with the server-list account name.

If `message != "SUCCESS"`, C++ sends `PLO_DISCMESSAGE` with that message, sets load-only, disconnects, and does not call `Player::sendLogin`.

If `message == "SUCCESS"`, C++ calls `Player::sendLogin`. The C# implementation now continues through the source-confirmed beginning of `Player::sendLogin` in `PlayerSendLoginContinuation`, then stops at `ReadyForWorldEntry` before `Server::playerLoggedIn`.

## `Player::sendLogin` Pre-World Checks

`Player::sendLogin` starts by loading the account file with `loadAccount(account, isRC || isNC)`. Confirmed rejection cases before the signature/world-entry path include:

- Account file marks `BANNED` and the account lacks `PLPERM_MODIFYSTAFFACCOUNT`: `"You have been banned.  Reason: {banReason with newline converted to carriage return after guntokenize}"`.
- RC/NC without staff rights or admin IP: `"You do not have RC rights."`.
- Client on `onlystaff=true` without staff rights: `"This server is currently restricted to staff only."`.
- Client admin IP mismatch unless `IPRANGE` contains `0.0.0.0`: `"Your IP doesn't match one of the allowed IPs for this account."`.
- Same non-guest account already in use by the same client family and active within 30 seconds: `"Account is already in use."`.

The implemented success-boundary path sends `PLO_SIGNATURE`, skips the unresolved login-server-name branch unless the missing `PLO_FULLSTOP` opcode is recovered, sends `PLO_UNKNOWN168` for clients, checks duplicate account sessions, and stops before registering the player with the list server through `Server::playerLoggedIn`. Everything after that point is beyond this milestone because it begins account state, world, level, props, RC, NC, and scripting behavior.
