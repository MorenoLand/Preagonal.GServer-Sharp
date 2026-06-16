using System.Text;
using GServ.Protocol;

namespace GServ.Game;

public sealed record RuntimeLevelItem(float X, float Y, LevelItemType ItemType);

public sealed record RuntimeHorse(string Image, float X, float Y, byte Direction, byte Bushes);

public enum BaddyMode : byte
{
    Walk = 0,
    SwampShot = 6,
    HareJump = 7
}

public sealed class RuntimeBaddy
{
    private static readonly string[] Images =
    [
        "baddygray.png", "baddyblue.png", "baddyred.png", "baddyblue.png", "baddygray.png",
        "baddyhare.png", "baddyoctopus.png", "baddygold.png", "baddylizardon.png", "baddydragon.png"
    ];

    private static readonly byte[] StartModes = [0, 0, 0, 0, 6, 7, 0, 0, 0, 0];
    private static readonly byte[] Powers = [2, 3, 4, 3, 2, 1, 1, 6, 12, 8];

    private RuntimeBaddy(byte id, float x, float y, byte type)
    {
        Id = id;
        X = x;
        Y = y;
        Type = type > Images.Length ? (byte)0 : type;
        Power = Powers[Type];
        Image = Images[Type];
        Mode = StartModes[Type];
        Direction = (2 << 2) | 2;
    }

    public byte Id { get; }
    public float X { get; }
    public float Y { get; }
    public byte Type { get; }
    public byte Power { get; }
    public string Image { get; }
    public byte Mode { get; }
    public byte Ani { get; } = 0;
    public byte Direction { get; }
    public IReadOnlyList<string> Verses { get; } = ["", "", ""];

    public static RuntimeBaddy Create(byte id, float x, float y, byte type) =>
        new(id, x, y, type);
}

public static class EntityRuntimePackets
{
    public static byte[] BaddyProps(RuntimeBaddy baddy, int clientVersion)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.BaddyProps);
        writer.WriteGChar(baddy.Id);
        WriteBaddyProps(writer, baddy, clientVersion);
        writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static void WriteBaddyProps(GraalBinaryWriter writer, RuntimeBaddy baddy, int clientVersion)
    {
        writer.WriteGChar(1);
        writer.WriteGChar((byte)(baddy.X * 2));
        writer.WriteGChar(2);
        writer.WriteGChar((byte)(baddy.Y * 2));
        writer.WriteGChar(3);
        writer.WriteGChar(baddy.Type);
        writer.WriteGChar(4);
        writer.WriteGChar(baddy.Power);
        var image = clientVersion < 210 ? baddy.Image.Replace(".png", ".gif", StringComparison.Ordinal) : baddy.Image;
        var imageBytes = Encoding.ASCII.GetBytes(image);
        writer.WriteGChar((byte)imageBytes.Length);
        writer.WriteBytes(imageBytes);
        writer.WriteGChar(5);
        writer.WriteGChar(baddy.Mode);
        writer.WriteGChar(6);
        writer.WriteGChar(baddy.Ani);
        writer.WriteGChar(7);
        writer.WriteGChar(baddy.Direction);

        for (byte propId = 8; propId <= 10; propId++)
        {
            writer.WriteGChar(propId);
            writer.WriteGChar(0);
        }
    }
}
