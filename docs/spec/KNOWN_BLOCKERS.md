# Spec Blockers

- Full `Player::sendLogin` is blocked on account file loading, player property serialization, world/level entry, file queue flush behavior, RC/NC login packet families, and optional scripting hooks.
- Real account/password validation must not be invented. The C++ server delegates password/auth verification to the list server through `SVO_VERIACC2`/`SVI_VERIACC2`.
- Account persistence behavior from `Account.cpp` is only partially traced and should be recovered before implementing production account services.
- `CFileQueue` compression and socket-level flush bytes for list-server auth are not implemented.
- Server-list connection lifecycle, reconnect backoff, registration, and text/listserver side channels need a dedicated milestone.
