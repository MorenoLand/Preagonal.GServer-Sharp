# WebSocket Frame Spec

## Source Of Truth

- `external/gs2lib/src/IUtil.cpp`
  - `webSocketFixOutgoingPacket`
  - `webSocketFixIncomingPacket`
- `external/gs2lib/include/IUtil.h`
- `external/gs2lib/src/CFileQueue.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`

## Confirmed Behavior

Outbound server frames:

- `CFileQueue::sendCompress` first builds the normal Graal socket payload for
  the selected encryption/compression generation.
- If `sock->webSocket` is true, it then calls
  `webSocketFixOutgoingPacket(oBuffer)`.
- The outgoing WebSocket frame is binary opcode with FIN set: first byte
  `0x82`.
- Payloads `<= 125` use a one-byte payload length.
- Payloads `126..65535` use marker `0x7E` followed by two big-endian length
  bytes.
- The server does not mask outgoing payload bytes.

Inbound client frames:

- `Player::onRecv` appends socket bytes to `m_recvBuffer`.
- If `m_playerSock->webSocket` is true, C++ calls
  `webSocketFixIncomingPacket(m_recvBuffer)` before normal Graal frame parsing.
- Only binary opcode `0x02` is accepted.
- The helper reads the mask key at offset `2`, `4`, or `10` depending on the
  WebSocket length code.
- It ignores the declared payload length after locating the mask and decodes
  all bytes available after the mask key.
- On the local MSVC/Windows fixture build, a close frame beginning with `0x88`
  returns `-1` and leaves the input unchanged because the recovered code
  compares a signed `char` value against integer `136`.

## Golden Fixtures

Captured with `tools/gs2lib-fixtures` against recovered `gs2lib`.

```txt
websocket-out-small-abc
input: 61 62 63
output: 82 03 61 62 63
```

```txt
websocket-out-126-a
input: ASCII("a" repeated 126)
output prefix: 82 7E 00 7E
output payload: ASCII("a" repeated 126)
```

```txt
websocket-in-small-masked-abc
input: 82 83 01 02 03 04 60 60 60
result: 3
output: 61 62 63
```

```txt
websocket-in-126-masked-abc-extra
input: 82 FE 00 03 01 02 03 04 60 60 60 65
result: 4
output: 61 62 63 61
```

The `websocket-in-126-masked-abc-extra` fixture proves the recovered helper's
available-byte behavior: declared payload length is `3`, but four bytes after
the mask are decoded because the helper uses the buffer length.

```txt
websocket-in-close
input: 88 80 00 00 00 00
result: -1
output: 88 80 00 00 00 00
```

## C# Status

Implemented:

- `GraalWebSocketFrame.WrapOutgoingBinary`
- `GraalWebSocketFrame.UnwrapIncoming`
- optional `GraalFileQueue.FlushSocket(wrapWebSocket: true)` wrapping after
  Graal compression/encryption framing

Still blocked:

- HTTP `200 OK` response body for non-WebSocket `GET /`
- `101 Switching Protocols` handshake bytes
- `Sec-WebSocket-Accept` generation/header order in the production socket path
- production WebSocket session state integration
- TLS/WolfSSL transport behavior
- outbound payloads larger than `65535` bytes remain uncaptured because the
  recovered `webSocketFixOutgoingPacket` uses a fixed `65535` byte temporary
  buffer before its `127` length branch
