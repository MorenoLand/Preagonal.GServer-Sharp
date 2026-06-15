using System.Text;
using GServ.Protocol;
using Xunit;

namespace GServ.Protocol.Tests;

public sealed class ServerListAuthPacketTests
{
    [Fact]
    public void VerifyAccount2RequestMatchesCppFieldOrder()
    {
        var bytes = ServerListAuthPackets.VerifyAccount2Request(
            accountName: "Ruan",
            password: "pw",
            playerId: 7,
            type: PlayerSessionType.Client3,
            identity: "win");

        Assert.Equal(
            new byte[]
            {
                49,
                36, 82, 117, 97, 110,
                34, 112, 119,
                32, 39,
                64,
                32, 35, 119, 105, 110
            },
            bytes);
    }

    [Fact]
    public void VerifyAccount2ResponseParsesAccountIdTypeAndMessage()
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ListServerToServerPacketId.VerifyAccount2);
        writer.WriteGChar(4);
        writer.WriteBytes(Encoding.ASCII.GetBytes("Ruan"));
        writer.WriteGShort(7);
        writer.WriteGChar((byte)PlayerSessionType.Client3);
        writer.WriteBytes(Encoding.ASCII.GetBytes("SUCCESS"));

        var response = ServerListAuthPackets.ParseVerifyAccount2Response(writer.ToArray()[1..]);

        Assert.Equal("Ruan", response.AccountName);
        Assert.Equal(7, response.PlayerId);
        Assert.Equal(PlayerSessionType.Client3, response.Type);
        Assert.Equal("SUCCESS", response.Message);
        Assert.True(response.IsSuccess);
    }
}
