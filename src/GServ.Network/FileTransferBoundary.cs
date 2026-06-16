using GServ.Protocol;

namespace GServ.Network;

public sealed record ResourceFile(string Name, byte[] Data, long ModTime);

public interface IResourceFileSystem
{
    ResourceFile? Find(string file);
}

public enum FileTransferDecision
{
    SentFile,
    FileMissing,
    UpToDate
}

public sealed record FileTransferResult(
    FileTransferDecision Decision,
    bool Sent,
    IReadOnlyList<string> KnownFiles);

public static class FileTransferBoundary
{
    public static FileTransferResult HandleWantFile(
        ClientSessionSkeleton session,
        IResourceFileSystem fileSystem,
        string fileName,
        ClientVersionId clientVersion)
    {
        fileName = NormalizeOldClientFilename(fileName, clientVersion);
        return SendFile(session, fileSystem, fileName, clientVersion);
    }

    public static FileTransferResult HandleVerifyWantSend(
        ClientSessionSkeleton session,
        IResourceFileSystem fileSystem,
        uint clientChecksum,
        string fileName,
        ClientVersionId clientVersion)
    {
        var file = fileSystem.Find(fileName);
        var ignoreChecksum = Path.GetExtension(fileName) == ".gupd";
        if (!ignoreChecksum && file is not null && Crc32.Compute(file.Data) == clientChecksum)
        {
            session.QueuePacket(FileTransferPackets.FileUpToDate(fileName));
            return new FileTransferResult(FileTransferDecision.UpToDate, Sent: false, KnownFiles: []);
        }

        return SendFile(session, fileSystem, fileName, clientVersion);
    }

    private static FileTransferResult SendFile(
        ClientSessionSkeleton session,
        IResourceFileSystem fileSystem,
        string fileName,
        ClientVersionId clientVersion)
    {
        var knownFiles = new List<string> { fileName };
        var file = fileSystem.Find(fileName);
        if (file is null || file.Data.Length == 0)
        {
            session.QueuePacket(FileTransferPackets.FileSendFailed(fileName));
            return new FileTransferResult(FileTransferDecision.FileMissing, Sent: false, knownFiles);
        }

        var includeModTime = clientVersion >= ClientVersionId.Client21;
        var isBigFile = file.Data.Length > FileTransferPackets.ChunkSize;
        if (clientVersion < ClientVersionId.Client214)
        {
            if (file.Data.Length > 64000)
            {
                session.QueuePacket(FileTransferPackets.FileSendFailed(fileName));
                return new FileTransferResult(FileTransferDecision.FileMissing, Sent: false, knownFiles);
            }

            isBigFile = false;
        }

        if (isBigFile)
        {
            session.QueuePacket(FileTransferPackets.LargeFileStart(fileName));
            session.QueuePacket(FileTransferPackets.LargeFileSize(file.Data.Length));
        }

        var offset = 0;
        while (offset < file.Data.Length)
        {
            var sendSize = Math.Min(FileTransferPackets.ChunkSize, file.Data.Length - offset);
            if (clientVersion < ClientVersionId.Client214)
                sendSize = file.Data.Length - offset;

            session.QueuePacket(FileTransferPackets.BuildFileChunk(
                fileName,
                file.Data.AsSpan(offset, sendSize),
                file.ModTime,
                includeModTime));
            offset += sendSize;
        }

        if (isBigFile)
            session.QueuePacket(FileTransferPackets.LargeFileEnd(fileName));

        return new FileTransferResult(FileTransferDecision.SentFile, Sent: true, knownFiles);
    }

    private static string NormalizeOldClientFilename(string fileName, ClientVersionId clientVersion)
    {
        if (clientVersion < ClientVersionId.Client21 && Path.GetExtension(fileName).Length == 0)
            return fileName + ".gif";
        return fileName;
    }
}
