using System.Text;

namespace GServ.Protocol;

public sealed record ServerListVerifyAccount2Response(
    string AccountName,
    ushort PlayerId,
    PlayerSessionType Type,
    string Message)
{
    public bool IsSuccess => Message == "SUCCESS";
}

public static class ServerListAuthPackets
{
    public static byte[] VerifyAccount2Request(
        string accountName,
        string password,
        ushort playerId,
        PlayerSessionType type,
        string identity)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToListServerPacketId.VerifyAccount2);
        WriteGCharString(writer, accountName);
        WriteGCharString(writer, password);
        writer.WriteGShort(playerId);
        writer.WriteGChar((byte)type);
        writer.WriteGShort((ushort)Encoding.ASCII.GetByteCount(identity));
        writer.WriteBytes(Encoding.ASCII.GetBytes(identity));
        return writer.ToArray();
    }

    public static ServerListVerifyAccount2Response ParseVerifyAccount2Response(ReadOnlySpan<byte> payloadWithoutPacketId)
    {
        var reader = new GraalBinaryReader(payloadWithoutPacketId);
        var account = ReadGCharString(reader);
        var id = reader.ReadGShort();
        var type = (PlayerSessionType)reader.ReadGChar();
        var message = Encoding.ASCII.GetString(reader.ReadBytes(reader.BytesLeft));
        return new ServerListVerifyAccount2Response(account, id, type, message);
    }

    private static void WriteGCharString(GraalBinaryWriter writer, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        writer.WriteGChar((byte)bytes.Length);
        writer.WriteBytes(bytes);
    }

    private static string ReadGCharString(GraalBinaryReader reader) =>
        Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadGChar()));
}
