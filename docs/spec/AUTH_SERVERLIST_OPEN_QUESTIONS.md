# Auth And Server-List Open Questions

- Exact production account validation remains blocked on a full `Account::loadAccount` and account-file compatibility pass.
- The exact list-server authentication authority is external; local C# should model it through an interface until live/list-server behavior is captured.
- `Player::sendLogin` success enters account props, `Server::playerLoggedIn`, world/level warp, file sending, RC/NC flows, and optional scripting events. It is intentionally not implemented here.
- The exact newline/compression bytes for flushed `SVO_VERIACC2` need a full `CFileQueue` zlib fixture before socket-level integration.
- The banned-account rejection message applies `guntokenize().replaceAll("\n", "\r")`; exact tokenization fixtures are not implemented yet.

