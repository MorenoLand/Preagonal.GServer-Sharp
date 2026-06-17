using System.Net;
using System.Net.Sockets;
using GServ.Protocol;
using Xunit;

namespace GServ.Network.Tests;

public sealed class ProductionServerListTcpSocketTests
{
    [Fact]
    public async Task SendPacketWritesConfirmedGen1AndGen2ListServerFramesToTcpStream()
    {
        using var listener = new TcpListener(IPAddress.Loopback, port: 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port.ToString();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var socket = new ProductionServerListTcpSocket();

        Assert.True(socket.Initialize(IPAddress.Loopback.ToString(), port));
        var acceptTask = listener.AcceptTcpClientAsync(timeout.Token);
        Assert.True(socket.Connect());
        using var serverSide = await acceptTask;
        await using var stream = serverSide.GetStream();

        socket.SetCodec(EncryptionGeneration.Gen1, key: 0);
        socket.SendPacket(ServerListAuthPackets.RegisterV3("3.0.9-beta"), sendNow: true);

        var gen1 = new byte[12];
        await stream.ReadExactlyAsync(gen1, timeout.Token);
        Assert.Equal(">" + "3.0.9-beta\n", System.Text.Encoding.ASCII.GetString(gen1));

        socket.SetCodec(EncryptionGeneration.Gen2, key: 0);
        socket.SendPacket(ServerListAuthPackets.ServerHqPass("secret"));

        var expectedQueue = new GraalFileQueue();
        expectedQueue.SetCodec(EncryptionGeneration.Gen2, key: 0);
        expectedQueue.AddRawPacket("7secret\n"u8);
        var expectedGen2 = expectedQueue.FlushSocket();

        var gen2 = new byte[expectedGen2.Length];
        await stream.ReadExactlyAsync(gen2, timeout.Token);
        Assert.Equal(expectedGen2, gen2);
    }
}
