using GServ.Game;
using GServ.Protocol;

namespace GServ.Game.Tests;

public sealed class EntityRuntimeBoundaryTests
{
    [Fact]
    public void RuntimeLevelItemsAppendRemoveFirstMatchingAndReportInvalidWhenMissing()
    {
        var level = new RuntimeLevel("start.nw");

        Assert.True(level.AddItem(10.5f, 11.5f, LevelItemType.RedRupee));
        Assert.True(level.AddItem(12.5f, 13.5f, LevelItemType.Bombs));
        Assert.Equal([LevelItemType.RedRupee, LevelItemType.Bombs], level.Items.Select(item => item.ItemType));

        Assert.Equal(LevelItemType.RedRupee, level.RemoveItem(10.5f, 11.5f));
        Assert.Equal([LevelItemType.Bombs], level.Items.Select(item => item.ItemType));
        Assert.Equal(LevelItemType.Invalid, level.RemoveItem(10.5f, 11.5f));
    }

    [Fact]
    public void RuntimeLevelHorsesAppendAndRemoveFirstMatchingCoordinates()
    {
        var level = new RuntimeLevel("start.nw");

        level.AddHorse("horse.png", 30.5f, 31.0f, direction: 2, bushes: 1);
        level.AddHorse("other.png", 10.0f, 11.0f, direction: 0, bushes: 0);

        level.RemoveHorse(30.5f, 31.0f);

        Assert.Equal(["other.png"], level.Horses.Select(horse => horse.Image));
    }

    [Fact]
    public void RuntimeLevelNpcsUseSetSemanticsAndSortedIteration()
    {
        var level = new RuntimeLevel("start.nw");

        Assert.True(level.AddNpc(9));
        Assert.True(level.AddNpc(3));
        Assert.False(level.AddNpc(9));

        Assert.Equal([3u, 9u], level.NpcIds);

        level.RemoveNpc(3);

        Assert.Equal([9u], level.NpcIds);
    }

    [Fact]
    public void RuntimeBaddiesStartAtOneReuseFreedIdsAndKeepCppFiftyOneBaddyBoundary()
    {
        var level = new RuntimeLevel("start.nw");

        var first = level.AddBaddy(10, 11, type: 2);
        var second = level.AddBaddy(12, 13, type: 3);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);

        level.RemoveBaddy(1);
        var reused = level.AddBaddy(14, 15, type: 4);

        Assert.NotNull(reused);
        Assert.Equal(1, reused.Id);

        while (level.Baddies.Count <= 50)
        {
            Assert.NotNull(level.AddBaddy(1, 1, type: 0));
        }

        Assert.Equal(51, level.Baddies.Count);
        Assert.Null(level.AddBaddy(1, 1, type: 0));
    }

    [Fact]
    public void LevelItemRupeeCountsMatchCppTable()
    {
        Assert.Equal(1, LevelItemCatalog.GetRupeeCount(LevelItemType.GreenRupee));
        Assert.Equal(5, LevelItemCatalog.GetRupeeCount(LevelItemType.BlueRupee));
        Assert.Equal(30, LevelItemCatalog.GetRupeeCount(LevelItemType.RedRupee));
        Assert.Equal(100, LevelItemCatalog.GetRupeeCount(LevelItemType.GoldRupee));
        Assert.Equal(0, LevelItemCatalog.GetRupeeCount(LevelItemType.Bombs));
    }

    [Fact]
    public void BaddyPropsMatchCppResetDefaultsAndPropertyOrder()
    {
        var baddy = RuntimeBaddy.Create(id: 1, x: 10, y: 11, type: 2);

        Assert.Equal(
            new byte[]
            {
                34, 33,
                33, 52,
                34, 54,
                35, 34,
                36, 36, 44, 98, 97, 100, 100, 121, 114, 101, 100, 46, 112, 110, 103,
                37, 32,
                38, 32,
                39, 42,
                40, 32,
                41, 32,
                42, 32,
                10
            },
            EntityRuntimePackets.BaddyProps(baddy, clientVersion: 217));
    }
}
