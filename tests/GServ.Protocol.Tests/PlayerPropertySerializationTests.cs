using GServ.Protocol;
using Xunit;

namespace GServ.Protocol.Tests;

public sealed class PlayerPropertySerializationTests
{
    [Fact]
    public void ConfirmedLoginSubsetUsesAscendingPropertyOrder()
    {
        var source = BaseSource();
        var bytes = PlayerPropertySerializer.SerializeConfirmedLoginSubset(
            source,
            [
                PlayerPropertyId.AccountName,
                PlayerPropertyId.CurrentPower,
                PlayerPropertyId.MaxPower
            ]);

        Assert.Equal(
            new byte[]
            {
                33, 35,
                34, 40,
                66, 39, (byte)'p', (byte)'c', (byte)':', (byte)'R', (byte)'u', (byte)'a', (byte)'n'
            },
            bytes);
    }

    [Fact]
    public void ConfirmedScalarAndStringPropertiesMatchGetPropEncodings()
    {
        var source = BaseSource();
        var bytes = PlayerPropertySerializer.SerializeConfirmedLoginSubset(
            source,
            [
                PlayerPropertyId.RupeesCount,
                PlayerPropertyId.SwordPower,
                PlayerPropertyId.CurrentLevel,
                PlayerPropertyId.IpAddress
            ]);

        Assert.Equal(
            new byte[]
            {
                35, 32, 41, 114,
                40, 64, 41, (byte)'s', (byte)'w', (byte)'o', (byte)'r', (byte)'d', (byte)'.', (byte)'p', (byte)'n', (byte)'g',
                52, 40, (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'.', (byte)'n', (byte)'w',
                62, 32, 32, 32, 32, 33
            },
            bytes);
    }

    [Fact]
    public void PlayerPropsPacketWrapsConfirmedSubsetWithPloPlayerpropsAndNewline()
    {
        var source = BaseSource();
        var payload = PlayerPropertySerializer.SerializeConfirmedLoginSubset(
            source,
            [PlayerPropertyId.MaxPower, PlayerPropertyId.CurrentPower]);

        Assert.Equal(
            new byte[] { 41, 33, 35, 34, 40, 10 },
            PlayerPropertySerializer.BuildPlayerPropsPacket(payload, appendNewline: true));
    }

    private static PlayerPropertySource BaseSource() =>
        new(
            Nickname: "Ruan",
            MaxPower: 3,
            Hitpoints: 4.0f,
            Rupees: 1234,
            Arrows: 30,
            Bombs: 8,
            GlovePower: 2,
            SwordPower: 2,
            SwordImage: "sword.png",
            ShieldPower: 1,
            ShieldImage: "shield.png",
            Gani: "idle",
            HeadImage: "head1.png",
            ChatMessage: "hi",
            Colors: [0, 1, 2, 3, 4],
            PlayerId: 7,
            X: 560,
            Y: 568,
            Sprite: 2,
            Status: 1,
            CarrySprite: 0,
            CurrentLevel: "start.nw",
            HorseImage: "horse.png",
            HorseBombCount: 0,
            CarryNpcId: 0,
            ApCounter: 4,
            MagicPoints: 7,
            Kills: 11,
            Deaths: 12,
            OnlineSeconds: 99,
            AccountIp: 1,
            Alignment: 40,
            AdditionalFlags: 0,
            AccountName: "pc:Ruan",
            BodyImage: "body.png",
            EloRating: 1500,
            EloDeviation: 50,
            GaniAttributes: new Dictionary<int, string>
            {
                [37] = "attr1",
                [38] = "attr2"
            },
            Os: "win",
            TextCodePage: 1252,
            CommunityName: "Ruan");
}
