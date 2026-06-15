# Golden Fixtures

All byte values are decimal unless noted.

## Login Packets

### PLO_SIGNATURE

C++:

```cpp
sendPacket(CString() >> (char)PLO_SIGNATURE >> (char)73);
```

Packet bytes before queue newline/compression:

```txt
[57, 105]
```

With `Player::sendPacket` newline append:

```txt
[57, 105, 10]
```

### PLO_UNKNOWN168

C++:

```cpp
sendPacket(CString() >> (char)PLO_UNKNOWN168);
```

`PLO_UNKNOWN168 = 168`; `GCHAR 168` writes `168 + 32 = 200`.

Packet bytes before queue newline/compression:

```txt
[200]
```

With `Player::sendPacket` newline append:

```txt
[200, 10]
```

### PLO_DISCMESSAGE

C++:

```cpp
sendPacket(CString() >> (char)PLO_DISCMESSAGE << "No");
```

Packet bytes before queue newline/compression:

```txt
[48, 78, 111]
```

With `Player::sendPacket` newline append:

```txt
[48, 78, 111, 10]
```

### Unknown Login Type

Input login prelude:

```txt
GCHAR 9 => raw byte [41], m_type = 1 << 9 = 512
```

Confirmed response:

```txt
[48] + ASCII("Your client type is unknown.  Please inform the OpenGraal Team.  Type: 512.") + [10]
```

## Player::sendLogin Pre-World Boundary

Normal client success continuation before `Server::playerLoggedIn`:

```txt
[57, 105, 10, 200, 10]
```

This is:

```txt
PLO_SIGNATURE, GCHAR 73, "\n", PLO_UNKNOWN168, "\n"
```

Active duplicate client rejection preserves C++ ordering, with early client packets already queued:

```txt
[57, 105, 10, 200, 10] + [48] + ASCII("Account is already in use.") + [10]
```

Banned account example with ban reason `"cheating"`:

```txt
[48] + ASCII("You have been banned.  Reason: cheating") + [10]
```

## Framing

Outer socket frame:

```txt
[0, 3, 97, 98, 99] => one inner frame "abc"
```

Raw-data transition:

```txt
PLI_RAWDATA GINT(4) "\n" "abc\n"
```

With client/newer-RC raw newline stripping enabled, raw payload becomes:

```txt
"abc"
```

## Server-List Auth

### SVO_VERIACC2

Input:

```txt
account="Ruan", password="pw", playerId=7, type=PLTYPE_CLIENT3, identity="win"
```

Packet body before `ServerList::sendPacket` newline and `CFileQueue` compression:

```txt
[49, 36, 82, 117, 97, 110, 34, 112, 119, 32, 39, 64, 32, 35, 119, 105, 110]
```

Notes:

- `49` is `GCHAR SVO_VERIACC2` (`17 + 32`).
- `64` is `GCHAR PLTYPE_CLIENT3` (`32 + 32`), because C++ sends the type bitfield, not the login exponent.

### SVI_VERIACC2 Failure

For `message != "SUCCESS"`, C++ sends the message directly as:

```txt
PLO_DISCMESSAGE + message + "\n"
```

Example message `"Bad password."`:

```txt
[48] + ASCII("Bad password.") + [10]
```
