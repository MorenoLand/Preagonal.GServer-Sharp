# Known Blockers

- Exact original `gs2compiler` submodule commit is not present in this fresh source snapshot. The repository URL is confirmed and current source was cloned, but scripting work should recover the exact commit before implementing runtime behavior.
- Full `IEnums.h` packet catalog is large; only foundation-critical IDs are implemented in C# so far.
- Full login success is blocked on production account/default account loading, player property emission, `Server::playerLoggedIn`, `sendLoginClient`/`sendLoginRC`/`sendLoginNC`, and world warp behavior.
- The login packet parse boundary, server-list auth boundary, and source-confirmed beginning of `Player::sendLogin` are implemented. The current stop point is `ReadyForWorldEntry`, immediately before `Server::playerLoggedIn(shared_from_this())`.
- The login-server-name branch in `Player::sendLogin` is blocked because C++ references `PLO_FULLSTOP`, but recovered `IEnums.h` only defines `PLO_FULLSTOP2 = 177`.
- `CFileQueue` compression and socket flushing are documented but not implemented in C# yet.
- WebSocket handling is gated by `WOLFSSL_ENABLED` code paths and needs a dedicated pass.
- `Server::doMain()` timing branches need a dedicated timing recovery pass.
- Gameplay systems, account persistence, RC/NC file browser, server-list protocol, and scripting bindings are not implemented.
