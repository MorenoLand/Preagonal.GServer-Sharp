# Build And Dependency Research

The C++ project is `GS2Emu VERSION 3.0.9`, described by CMake as a Graal Online `v1.411` to `v6.037` compatible server.

Confirmed build options:

- `STATIC` defaults `ON`.
- `GRALATNPC` defaults `ON`, useful only with V8 NPC-server.
- `V8NPCSERVER` defaults `OFF`.
- `TESTS` defaults `ON`.
- `UPNP` defaults `OFF`.

Confirmed dependency declarations:

- Required submodules:
  - `dependencies/gs2lib`: `https://xtjoeytx@bitbucket.org/xtjoeytx/gs2lib.git`
  - `dependencies/gs2compiler`: `https://github.com/xtjoeytx/gs2-parser.git`
- Optional submodules:
  - `dependencies/miniupnp`: `https://github.com/miniupnp/miniupnp.git`
  - `dependencies/depot_tools`: `https://chromium.googlesource.com/chromium/tools/depot_tools.git`
  - `cmake/nuget`: `https://github.com/katusk/CMakeNuGetTools`
  - `vcpkg`: `https://github.com/microsoft/vcpkg.git`
- `V8NPCSERVER=ON` additionally uses V8, OpenSSL, optional zstd, and `cpp-httplib` at commit `a609330e4c6374f741d3b369269f7848255e1954`.
- C++ tests use Catch2 `v3.4.0`.

Recovery status:

- `gs2lib` was recovered exactly at commit `63b1ae96491c188905b50c6b61c8532c601a2122` and contains all protocol-critical headers.
- `gs2compiler` was cloned from the declared URL at recovered commit
  `4fa0a26ca75ac5238fe34a1d90ef9a459b02c2f9`, but the original submodule
  commit was not present in this fresh snapshot. Treat this commit as a
  supporting reference until the exact submodule pointer is recovered.
- Optional V8/miniupnp/depot/vcpkg sources were not cloned for this phase because their exact commits were not required for confirmed protocol primitives.
