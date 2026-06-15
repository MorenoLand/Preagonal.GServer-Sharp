using System.Text;
using GServ.Protocol;

namespace GServ.Network;

public sealed record LoginFlag(string Name, string Value);

public sealed record PostLoginPlayerSnapshot(
    ushort PlayerId,
    PlayerSessionType Type,
    byte[] AccountNameProperty,
    byte[] NicknameProperty,
    byte[] CurrentLevelProperty,
    byte[] XProperty,
    byte[] YProperty,
    byte[] AlignmentProperty,
    byte[] IpAddressProperty,
    byte[] LoginPropertiesPayload,
    IReadOnlyList<LoginFlag> PlayerFlags,
    IReadOnlyList<LoginFlag> ServerFlags);

public enum PostLoginClientStopPoint
{
    BeforeWarp
}

public sealed record PostLoginClientBoundaryResult(
    bool Accepted,
    byte[] ServerListAddPlayerPacket,
    PostLoginClientStopPoint StopPoint);

public static class PostLoginWorldEntryBoundary
{
    public static PostLoginClientBoundaryResult BeginClient(
        ClientSessionSkeleton session,
        PostLoginPlayerSnapshot snapshot)
    {
        if (session.Lifecycle != SessionLifecycle.ReadyForWorldEntry)
            throw new InvalidOperationException("sendLoginClient boundary requires ReadyForWorldEntry.");

        var serverListAddPlayerPacket = BuildServerListAddPlayerPacket(snapshot);

        session.QueuePacket(PlayerProperties(snapshot.LoginPropertiesPayload, appendNewline: true));
        session.QueuePacket(BlankPacket(ServerToPlayerPacketId.ClearWeapons, appendNewline: true));

        foreach (var flag in snapshot.PlayerFlags)
            session.QueuePacket(FlagSet(flag, appendNewline: true));

        foreach (var flag in snapshot.ServerFlags)
            session.QueuePacket(FlagSet(flag, appendNewline: true));

        session.QueuePacket(NpcWeaponDelete("Bomb", appendNewline: true));
        session.QueuePacket(NpcWeaponDelete("Bow", appendNewline: true));
        session.QueuePacket(BlankPacket(ServerToPlayerPacketId.ServerListConnected, appendNewline: true));
        session.MarkReadyForLevelWarp();

        return new PostLoginClientBoundaryResult(
            true,
            serverListAddPlayerPacket,
            PostLoginClientStopPoint.BeforeWarp);
    }

    public static byte[] BuildServerListAddPlayerPacket(PostLoginPlayerSnapshot snapshot)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToListServerPacketId.PlayerAdd);
        writer.WriteRawShort(snapshot.PlayerId);
        writer.WriteGChar((byte)snapshot.Type);
        WriteProperty(writer, 34, snapshot.AccountNameProperty);
        WriteProperty(writer, 0, snapshot.NicknameProperty);
        WriteProperty(writer, 20, snapshot.CurrentLevelProperty);
        WriteProperty(writer, 15, snapshot.XProperty);
        WriteProperty(writer, 16, snapshot.YProperty);
        WriteProperty(writer, 32, snapshot.AlignmentProperty);
        WriteProperty(writer, 30, snapshot.IpAddressProperty);
        return writer.ToArray();
    }

    private static byte[] PlayerProperties(ReadOnlySpan<byte> propertyPayload, bool appendNewline)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.PlayerProps);
        writer.WriteBytes(propertyPayload);
        if (appendNewline)
            writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static byte[] BlankPacket(ServerToPlayerPacketId packetId, bool appendNewline)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)packetId);
        if (appendNewline)
            writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static byte[] FlagSet(LoginFlag flag, bool appendNewline)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.FlagSet);
        writer.WriteBytes(Encoding.ASCII.GetBytes(flag.Value.Length == 0 ? flag.Name : $"{flag.Name}={flag.Value}"));
        if (appendNewline)
            writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static byte[] NpcWeaponDelete(string weaponName, bool appendNewline)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.NpcWeaponDelete);
        writer.WriteBytes(Encoding.ASCII.GetBytes(weaponName));
        if (appendNewline)
            writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static void WriteProperty(GraalBinaryWriter writer, byte propertyId, ReadOnlySpan<byte> payload)
    {
        writer.WriteGChar(propertyId);
        writer.WriteBytes(payload);
    }
}
