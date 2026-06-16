using GServ.Game;

namespace GServ.Game.Tests;

public sealed class LevelInteractionBoundaryTests
{
    [Fact]
    public void FindTouchedLinkUsesCppInclusiveTileBoundsAndLevelOrder()
    {
        var level = new NwLevelSnapshot(
            "GLEVNW01",
            [
                new NwLevelLink("first.nw", 1, 2, 3, 4, "5", "6"),
                new NwLevelLink("second.nw", 4, 6, 2, 2, "7", "8")
            ],
            [],
            [],
            [],
            []);

        Assert.Equal("first.nw", LevelInteraction.FindTouchedLink(level, 1, 2)?.NewLevel);
        Assert.Equal("first.nw", LevelInteraction.FindTouchedLink(level, 4, 6)?.NewLevel);
        Assert.Equal("second.nw", LevelInteraction.FindTouchedLink(level, 5, 6)?.NewLevel);
        Assert.Null(LevelInteraction.FindTouchedLink(level, 7, 6));
        Assert.Null(LevelInteraction.FindTouchedLink(level, 4, 9));
    }

    [Fact]
    public void BuildChestKeyUsesCppXColonYColonLevelNameFormat()
    {
        var chest = new NwLevelChest(10, 11, LevelItemType.RedRupee, 3);

        Assert.Equal("10:11:start.nw", LevelInteraction.BuildChestKey(chest, "start.nw"));
    }

    [Fact]
    public void TryOpenChestBuildsCppAckPacketAndRecordsUnopenedChest()
    {
        var level = new NwLevelSnapshot(
            "GLEVNW01",
            [],
            [],
            [],
            [],
            [new NwLevelChest(10, 11, LevelItemType.RedRupee, 3)]);
        var opened = new HashSet<string>(StringComparer.Ordinal);

        var result = LevelInteraction.TryOpenChest(level, "start.nw", 10, 11, opened);

        Assert.True(result.Opened);
        Assert.Equal("10:11:start.nw", result.ChestKey);
        Assert.Equal(LevelItemType.RedRupee, result.ItemType);
        Assert.Equal([36, 33, 42, 43, 10], result.Packet);
        Assert.Contains("10:11:start.nw", opened);
    }

    [Fact]
    public void TryOpenChestSkipsMissingOrAlreadyOpenedChest()
    {
        var level = new NwLevelSnapshot(
            "GLEVNW01",
            [],
            [],
            [],
            [],
            [new NwLevelChest(10, 11, LevelItemType.RedRupee, 3)]);
        var opened = new HashSet<string>(["10:11:start.nw"], StringComparer.Ordinal);

        var alreadyOpened = LevelInteraction.TryOpenChest(level, "start.nw", 10, 11, opened);
        var missing = LevelInteraction.TryOpenChest(level, "start.nw", 12, 13, opened);

        Assert.False(alreadyOpened.Opened);
        Assert.Empty(alreadyOpened.Packet);
        Assert.False(missing.Opened);
        Assert.Empty(missing.Packet);
        Assert.Single(opened);
    }
}
