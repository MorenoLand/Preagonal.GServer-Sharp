using GServ.Network;
using GServ.Protocol;
using Xunit;

namespace GServ.Network.Tests;

public sealed class PlayerSendLoginContinuationTests
{
    [Fact]
    public void BannedAccountRejectsBeforeEarlyLoginPackets()
    {
        var session = AuthenticatedClient3Session();
        var account = BaseAccount() with { IsBanned = true, BanReason = "cheating" };

        var result = PlayerSendLoginContinuation.Begin(session, account, BaseOptions());

        Assert.False(result.Accepted);
        Assert.Equal(SessionLifecycle.Rejected, session.Lifecycle);
        Assert.Equal(
            OutboundLoginPackets.DisconnectMessage("You have been banned.  Reason: cheating", appendNewline: true),
            session.TakeOutboundBytes());
    }

    [Fact]
    public void StaffOnlyServerRejectsNonStaffClientBeforeEarlyLoginPackets()
    {
        var session = AuthenticatedClient3Session();
        var options = BaseOptions() with { OnlyStaff = true };

        var result = PlayerSendLoginContinuation.Begin(session, BaseAccount(), options);

        Assert.False(result.Accepted);
        Assert.Equal(
            OutboundLoginPackets.DisconnectMessage("This server is currently restricted to staff only.", appendNewline: true),
            session.TakeOutboundBytes());
    }

    [Fact]
    public void AdminIpMismatchRejectsClientBeforeEarlyLoginPackets()
    {
        var session = AuthenticatedClient3Session();
        var account = BaseAccount() with { IsAdminIp = false, AdminIps = ["127.0.0.1"] };

        var result = PlayerSendLoginContinuation.Begin(session, account, BaseOptions());

        Assert.False(result.Accepted);
        Assert.Equal(
            OutboundLoginPackets.DisconnectMessage("Your IP doesn't match one of the allowed IPs for this account.", appendNewline: true),
            session.TakeOutboundBytes());
    }

    [Fact]
    public void RcLoginWithoutStaffRightsUsesRcRightsDisconnectMessage()
    {
        var session = AuthenticatedRemoteControlSession();
        var account = BaseAccount() with { IsStaff = false };

        var result = PlayerSendLoginContinuation.Begin(session, account, BaseOptions());

        Assert.False(result.Accepted);
        Assert.Equal(
            OutboundLoginPackets.DisconnectMessage("You do not have RC rights.", appendNewline: true),
            session.TakeOutboundBytes());
    }

    [Fact]
    public void ClientSuccessQueuesSignatureAndUnknown168ThenStopsBeforeWorldEntry()
    {
        var session = AuthenticatedClient3Session();

        var result = PlayerSendLoginContinuation.Begin(session, BaseAccount(), BaseOptions());

        Assert.True(result.Accepted);
        Assert.Equal(SessionLifecycle.ReadyForWorldEntry, session.Lifecycle);
        Assert.Equal(
            OutboundLoginPackets.Signature(appendNewline: true)
                .Concat(OutboundLoginPackets.Unknown168(appendNewline: true))
                .ToArray(),
            session.TakeOutboundBytes());
        Assert.Empty(result.DuplicateDisconnects);
    }

    [Fact]
    public void ActiveDuplicateClientRejectsAfterEarlyClientPackets()
    {
        var session = AuthenticatedClient3Session();
        var options = BaseOptions() with
        {
            ActiveSessions =
            [
                new ActivePlayerSession(12, "PC:RUAN", PlayerSessionType.Client3, TimeSpan.FromSeconds(5))
            ]
        };

        var result = PlayerSendLoginContinuation.Begin(session, BaseAccount(), options);

        Assert.False(result.Accepted);
        Assert.Equal(SessionLifecycle.Rejected, session.Lifecycle);
        Assert.Equal(
            OutboundLoginPackets.Signature(appendNewline: true)
                .Concat(OutboundLoginPackets.Unknown168(appendNewline: true))
                .Concat(OutboundLoginPackets.DisconnectMessage("Account is already in use.", appendNewline: true))
                .ToArray(),
            session.TakeOutboundBytes());
    }

    [Fact]
    public void StaleDuplicateClientIsMarkedForDisconnectAndCurrentSessionContinues()
    {
        var session = AuthenticatedClient3Session();
        var options = BaseOptions() with
        {
            ActiveSessions =
            [
                new ActivePlayerSession(12, "pc:Ruan", PlayerSessionType.Client3, TimeSpan.FromSeconds(31))
            ]
        };

        var result = PlayerSendLoginContinuation.Begin(session, BaseAccount(), options);

        Assert.True(result.Accepted);
        var duplicate = Assert.Single(result.DuplicateDisconnects);
        Assert.Equal(12, duplicate.SessionId);
        Assert.Equal("Someone else has logged into your account.", duplicate.Message);
        Assert.Equal(SessionLifecycle.ReadyForWorldEntry, session.Lifecycle);
    }

    private static PlayerSendLoginAccount BaseAccount() =>
        new(
            AccountName: "pc:Ruan",
            IsBanned: false,
            BanReason: "",
            HasModifyStaffAccountRight: false,
            IsStaff: false,
            IsAdminIp: true,
            AdminIps: ["0.0.0.0"],
            IsGuest: false);

    private static PlayerSendLoginOptions BaseOptions() =>
        new(
            OnlyStaff: false,
            ServerName: "Graal Reborn",
            ActiveSessions: []);

    private static ClientSessionSkeleton AuthenticatedClient3Session()
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
        Assert.True(session.ReceiveServerListAuthResponse(
            new ServerListVerifyAccount2Response("pc:Ruan", 7, PlayerSessionType.Client3, "SUCCESS")));
        return session;
    }

    private static ClientSessionSkeleton AuthenticatedRemoteControlSession()
    {
        var session = new ClientSessionSkeleton(8);
        var packet = new GraalBinaryWriter();
        packet.WriteGChar(1);
        packet.WriteGChar(42);
        packet.WriteBytes("GNW2214"u8);
        packet.WriteGChar(4);
        packet.WriteBytes("Ruan"u8);
        packet.WriteGChar(2);
        packet.WriteBytes("pw"u8);
        packet.WriteBytes("win"u8);
        Assert.True(session.ReceiveLoginPacket(packet.ToArray()));
        Assert.True(session.ReceiveServerListAuthResponse(
            new ServerListVerifyAccount2Response("pc:Ruan", 8, PlayerSessionType.RemoteControl, "SUCCESS")));
        return session;
    }
}
