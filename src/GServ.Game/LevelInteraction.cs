using GServ.Protocol;
using System.Globalization;

namespace GServ.Game;

public sealed record LevelChestOpenResult(
    bool Opened,
    string ChestKey,
    LevelItemType ItemType,
    byte[] Packet)
{
    public static LevelChestOpenResult NotOpened { get; } =
        new(false, string.Empty, LevelItemType.Invalid, []);
}

public static class LevelInteraction
{
    public static NwLevelLink? FindTouchedLink(NwLevelSnapshot level, int tileX, int tileY)
    {
        foreach (var link in level.Links)
        {
            if (tileX >= link.X &&
                tileX <= link.X + link.Width &&
                tileY >= link.Y &&
                tileY <= link.Y + link.Height)
            {
                return link;
            }
        }

        return null;
    }

    public static string BuildChestKey(NwLevelChest chest, string levelName)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{chest.X}:{chest.Y}:{levelName}");
    }

    public static LevelChestOpenResult TryOpenChest(
        NwLevelSnapshot level,
        string levelName,
        byte x,
        byte y,
        ISet<string> openedChests)
    {
        foreach (var chest in level.Chests)
        {
            if (chest.X != x || chest.Y != y)
                continue;

            var chestKey = BuildChestKey(chest, levelName);
            if (openedChests.Contains(chestKey))
                return LevelChestOpenResult.NotOpened;

            openedChests.Add(chestKey);
            return new LevelChestOpenResult(true, chestKey, chest.ItemType, BuildOpenedChestPacket(x, y));
        }

        return LevelChestOpenResult.NotOpened;
    }

    private static byte[] BuildOpenedChestPacket(byte x, byte y)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.LevelChest);
        writer.WriteGChar(1);
        writer.WriteGChar(x);
        writer.WriteGChar(y);
        writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }
}
