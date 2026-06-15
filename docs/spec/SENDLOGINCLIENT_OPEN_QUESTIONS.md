# sendLoginClient Open Questions

- Full `sendProps(__sendLogin)` needs a dedicated player property pass before C# can compute the login prop payload from account/player fields.
- The exact account/default-account values used by `getProp` are still blocked on production account-file loading.
- Old-version map workaround uses `msgPLI_WANTFILE`; file/resource transfer and `CFileQueue` flush bytes need a separate milestone.
- Weapon/class/protected-weapon branches require `Weapon::getWeaponPacket`, `LevelItem::getItemId`, class packet construction, and script/file behavior.
- The zlib-fix NPC weapon branch embeds `_zlibFix`; this needs a version-specific golden fixture before implementation.
- `warp(m_levelName, getX(), getY())` begins real level/map runtime behavior and must be traced separately.
- `PLO_UNKNOWN190` in C++ maps to `PLO_SERVERLISTCONNECTED = 190` in recovered `IEnums.h`; docs should keep both names visible for source traceability.
