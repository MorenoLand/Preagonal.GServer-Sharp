# Player::sendLogin Open Questions

- `PLO_FULLSTOP` is referenced by `PlayerLogin.cpp` but is missing from recovered `external/gs2lib/include/IEnums.h`. The similarly named `PLO_FULLSTOP2 = 177` is not assumed to be the same packet.
- Exact `CString::guntokenize()` behavior for ban reasons should be fixture-tested before full account-file loading is implemented. The C# boundary currently preserves the confirmed newline-to-carriage-return replacement for supplied ban reason snapshots.
- Production `loadAccount` behavior still needs a dedicated porting pass, including default account fallback, guest account generation, rights parsing, and all account fields loaded before world entry.
- `Server::playerLoggedIn` begins list-server presence and optional NPC scripting callbacks; it must be traced before crossing the `ReadyForWorldEntry` boundary.
- `sendLoginClient`, `sendLoginRC`, and `sendLoginNC` are intentionally outside this milestone and contain player props, level/warp behavior, RC packet families, and script-visible side effects.
