# Auth And Server-List Open Questions

- Exact production account validation remains blocked on a full `Account::loadAccount` and account-file compatibility pass.
- The exact list-server authentication authority is external; local C# should model it through an interface until live/list-server behavior is captured.
- `Player::sendLogin` success enters account props, `Server::playerLoggedIn`, world/level warp, file sending, RC/NC flows, and optional scripting events. It is intentionally not implemented here.
- The exact zlib socket bytes for gen2 list-server `CFileQueue` flush are now
  fixture-confirmed. Real list-server integration still needs the production
  connection lifecycle and auth service boundary.
- The production auth boundary can now queue `SVO_VERIACC2` through an
  `IProductionServerListGateway`, but the actual socket connection/reconnect
  implementation is still not ported.
- `ServerList::connectServer` registration packet bodies are implemented, but
  local IP discovery and queue codec transition timing still need a production
  socket milestone before this can connect to a real list server.
- The banned-account rejection message applies `guntokenize().replaceAll("\n", "\r")`; exact tokenization fixtures are not implemented yet.
