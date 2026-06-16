# File Transfer Cache Spec

## Source Files

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerUpdatePackages.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Player.h`
- `external/gs2lib/src/CFileQueue.cpp`
- `external/gs2lib/include/CFileQueue.h`
- `external/gs2lib/src/CString.cpp`
- `external/gs2lib/include/IEnums.h`

## Confirmed Packet IDs

- `PLI_WANTFILE = 23`
- `PLI_VERIFYWANTSEND = 47`
- `PLI_UPDATEPACKAGEREQUESTFILE = 159`
- `PLO_FILESENDFAILED = 30`
- `PLO_FILEUPTODATE = 45`
- `PLO_LARGEFILESTART = 68`
- `PLO_LARGEFILEEND = 69`
- `PLO_LARGEFILESIZE = 84`
- `PLO_RAWDATA = 100`
- `PLO_FILE = 102`
- `PLO_UPDATEPACKAGESIZE = 105`
- `PLO_UPDATEPACKAGEDONE = 106`
- `PLO_UPDATEPACKAGEISUPDATED = 187`

## `Player::sendPacket`

`Player::sendPacket(CString pPacket, bool appendNL = true)` returns for empty
packets, appends `"\n"` when requested and missing, then calls
`m_fileQueue.addPacket(pPacket)`.

File transfer builders in C# produce the same queued packet bytes before socket
compression/encryption.

## `PLI_WANTFILE`

`Player::msgPLI_WANTFILE` reads the remaining packet as the requested file name.
For clients older than `CLVER_2_1`, if the requested file has no extension, C++
appends `.gif`. It then calls `sendFile(file)`.

`Player::sendFile(const CString& pFile)` records the file name in
`m_knownFiles` for clients before lookup. If `FileSystem::find(pFile)` returns
empty, it queues:

```txt
PLO_FILESENDFAILED + file + "\n"
```

If found, C++ strips the base path down to the resource-relative directory and
calls `sendFile(path, file)`.

## `sendFile(path, file)`

`Player::sendFile(path, file)` loads the file contents and file stat mod time.
If `fileData.length() == 0`, it queues `PLO_FILESENDFAILED + file + "\n"` and
returns false. Empty files are therefore treated as failed sends.

For non-empty files:

- `packetLength = 1 + 5 + 1 + file.length() + 1`
- files longer than `32000` bytes are large files for modern clients
- clients older than `CLVER_2_14` do not use the large-file branch
- clients older than `CLVER_2_1` do not receive file mod time
- old clients reject file payloads over `64000` bytes with `PLO_FILESENDFAILED`

Modern chunk payload:

```txt
PLO_RAWDATA + GINT(packetLength + chunkSize) + "\n"
PLO_FILE + GINT5(modTime) + GCHAR(file.length) + file + chunk + "\n"
```

Old-client chunk payload:

```txt
PLO_RAWDATA + GINT(packetLength - 1 - 5 + chunkSize) + "\n"
PLO_FILE + GCHAR(file.length) + file + chunk
```

The old-client raw payload omits both mod time and the trailing newline.

Large modern files queue before chunks:

```txt
PLO_LARGEFILESTART + file + "\n"
PLO_LARGEFILESIZE + GINT5(fileData.length) + "\n"
```

and after all chunks:

```txt
PLO_LARGEFILEEND + file + "\n"
```

## `PLI_VERIFYWANTSEND`

`Player::msgPLI_VERIFYWANTSEND` reads:

```txt
GINT5 crc32
remaining string fileName
```

If `getExtension(fileName) == ".gupd"`, checksum comparison is ignored and the
file is always sent. Otherwise C++ loads the file through the main filesystem;
if it exists and zlib `crc32(0, data)` matches the client checksum, it queues:

```txt
PLO_FILEUPTODATE + fileName + "\n"
```

If missing or checksum differs, it calls `sendFile(fileName)`.

## `PLI_UPDATEPACKAGEREQUESTFILE`

`Player::msgPLI_UPDATEPACKAGEREQUESTFILE` reads:

```txt
GCHAR packageNameLength
packageName
GCHAR installType
remaining string fileChecksums
```

`installType == 2` means reinstall and clears all supplied checksum data. For
each package file entry, if at least five checksum bytes remain, C++ reads one
GINT5 checksum and skips the file only when it equals the package entry
checksum. Otherwise that file is considered missing.

The confirmed packet sequence is:

```txt
PLO_UPDATEPACKAGESIZE + GCHAR(packageName.length) + packageName + GINT5(totalDownloadSize) + "\n"
for each missing file: sendFile(fileName)
PLO_UPDATEPACKAGEDONE + packageName + "\n"
m_fileQueue.sendCompress(true)
```

Package manager parsing, package file discovery, and production package
lifecycle are not implemented in C# yet.

## C# Boundary

Implemented:

- source-confirmed file transfer packet IDs
- `FileTransferPackets` builders for file failed, file up-to-date,
  raw-data/file chunks, large-file markers, and update-package size/done
- zlib-compatible CRC32 helper
- `FileTransferBoundary.HandleWantFile`
- `FileTransferBoundary.HandleVerifyWantSend`
- old-client `.gif` extension behavior
- empty/missing file failure behavior
- old-client no-modtime chunk behavior
- modern modtime chunk behavior
- modern large-file start/size/chunk/end sequencing
- `.gupd` checksum-ignore behavior for verify-want-send

Blocked:

- upload/write paths such as `PLI_UPDATEFILE` security and overwrite behavior
- update package manager/resource parsing and production registration
- exact `FileSystem::find` relative path stripping beyond the current resource
  abstraction
- default-file cache branch in `msgPLI_UPDATEFILE`
- integration with production socket loop beyond already-confirmed
  `GraalFileQueue` queue/compression behavior
