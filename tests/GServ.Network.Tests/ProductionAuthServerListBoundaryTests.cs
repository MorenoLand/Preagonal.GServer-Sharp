using GServ.Network;
using GServ.Protocol;
using Xunit;

namespace GServ.Network.Tests;

public sealed class ProductionAuthServerListBoundaryTests
{
    [Fact]
    public void BeginQueuesVerifyAccountRequestToGatewayAndWaitsForListServer()
    {
        var session = Client3Session();
        var gateway = new CapturingGateway(isConnected: true);
        var boundary = new ProductionAuthServerListBoundary(gateway, new PreWorldAuthOptions(
            MaxPlayers: 128,
            CurrentPlayerCount: 0,
            IsIpBanned: false,
            IsServerListConnected: gateway.IsConnected,
            AllowedVersions: ["G3D0311C"],
            AllowedVersionText: "6.037"));

        var result = boundary.Begin(session);

        Assert.True(result.Accepted);
        Assert.Equal(SessionLifecycle.WaitingForServerListAuth, session.Lifecycle);
        Assert.Equal(
            ServerListAuthPackets.VerifyAccount2Request("Ruan", "pw", 7, PlayerSessionType.Client3, "win"),
            gateway.LastLoginPacketForPlayer);
        Assert.Empty(session.TakeOutboundBytes());
    }

    [Fact]
    public void BeginDoesNotQueueGatewayPacketWhenPreWorldCheckRejects()
    {
        var session = Client3Session();
        var gateway = new CapturingGateway(isConnected: false);
        var boundary = new ProductionAuthServerListBoundary(gateway, new PreWorldAuthOptions(
            MaxPlayers: 128,
            CurrentPlayerCount: 0,
            IsIpBanned: false,
            IsServerListConnected: gateway.IsConnected,
            AllowedVersions: ["G3D0311C"],
            AllowedVersionText: "6.037"));

        var result = boundary.Begin(session);

        Assert.False(result.Accepted);
        Assert.Null(gateway.LastLoginPacketForPlayer);
        Assert.Equal(
            OutboundLoginPackets.DisconnectMessage("The login server is offline.  Try again later.", appendNewline: true),
            session.TakeOutboundBytes());
    }

    private static ClientSessionSkeleton Client3Session()
    {
        var session = new ClientSessionSkeleton(7);
        var packet = new GraalBinaryWriter();
        packet.WriteGChar(5);
        packet.WriteGChar(42);
        packet.WriteBytes("G3D0311C"u8);
        packet.WriteGChar(4);
        packet.WriteBytes("Ruan"u8);
        packet.WriteGChar(2);
        packet.WriteBytes("pw"u8);
        packet.WriteBytes("win"u8);
        Assert.True(session.ReceiveLoginPacket(packet.ToArray()));
        return session;
    }

    private sealed class CapturingGateway(bool isConnected) : IProductionServerListGateway
    {
        public bool IsConnected { get; } = isConnected;
        public byte[]? LastLoginPacketForPlayer { get; private set; }

        public void SendLoginPacketForPlayer(byte[] packetBody)
        {
            LastLoginPacketForPlayer = packetBody;
        }
    }
}
