using System.Text;
using GServ.Network;
using GServ.Protocol;
using Xunit;

namespace GServ.Network.Tests;

public sealed class FileTransferBoundaryTests
{
    [Fact]
    public void WantFileAppendsGifForOldClientsWithoutExtension()
    {
        var fileSystem = new MemoryResourceFileSystem();
        fileSystem.Add("head.gif", Encoding.ASCII.GetBytes("x"), modTime: 1);
        var session = new ClientSessionSkeleton(7);

        var result = FileTransferBoundary.HandleWantFile(
            session,
            fileSystem,
            "head",
            ClientVersionId.Client1411);

        Assert.True(result.Sent);
        Assert.Contains("head.gif", result.KnownFiles);
        Assert.Equal([132, 32, 32, 43, 10, 134, 40, 104, 101, 97, 100, 46, 103, 105, 102, 120], session.TakeOutboundBytes());
    }

    [Fact]
    public void WantFileMissingQueuesFileSendFailedButStillRecordsKnownClientFile()
    {
        var session = new ClientSessionSkeleton(7);

        var result = FileTransferBoundary.HandleWantFile(
            session,
            new MemoryResourceFileSystem(),
            "missing.png",
            ClientVersionId.Client6037);

        Assert.False(result.Sent);
        Assert.Contains("missing.png", result.KnownFiles);
        Assert.Equal([62, 109, 105, 115, 115, 105, 110, 103, 46, 112, 110, 103, 10], session.TakeOutboundBytes());
    }

    [Fact]
    public void VerifyWantSendQueuesUpToDateWhenChecksumMatchesNonGupdFile()
    {
        var fileSystem = new MemoryResourceFileSystem();
        fileSystem.Add("script.txt", Encoding.ASCII.GetBytes("abc"), modTime: 1);
        var session = new ClientSessionSkeleton(7);

        var result = FileTransferBoundary.HandleVerifyWantSend(
            session,
            fileSystem,
            Crc32.Compute(Encoding.ASCII.GetBytes("abc")),
            "script.txt",
            ClientVersionId.Client6037);

        Assert.Equal(FileTransferDecision.UpToDate, result.Decision);
        Assert.Equal([77, 115, 99, 114, 105, 112, 116, 46, 116, 120, 116, 10], session.TakeOutboundBytes());
    }

    [Fact]
    public void VerifyWantSendIgnoresChecksumForGupdAndSendsFile()
    {
        var fileSystem = new MemoryResourceFileSystem();
        fileSystem.Add("pack.gupd", Encoding.ASCII.GetBytes("abc"), modTime: 1);
        var session = new ClientSessionSkeleton(7);

        var result = FileTransferBoundary.HandleVerifyWantSend(
            session,
            fileSystem,
            Crc32.Compute(Encoding.ASCII.GetBytes("abc")),
            "pack.gupd",
            ClientVersionId.Client6037);

        Assert.Equal(FileTransferDecision.SentFile, result.Decision);
        Assert.Equal(2, session.TakeOutboundBytes().Count(value => value == 10));
    }

    private sealed class MemoryResourceFileSystem : IResourceFileSystem
    {
        private readonly Dictionary<string, ResourceFile> _files = new(StringComparer.Ordinal);

        public void Add(string name, byte[] data, long modTime) =>
            _files[name] = new ResourceFile(name, data, modTime);

        public ResourceFile? Find(string file) =>
            _files.GetValueOrDefault(file);
    }
}
