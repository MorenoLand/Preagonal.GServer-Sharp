using Preagonal.GameServer.Persistence;

namespace Preagonal.GameServer.Configuration;

public class Settings
{
	public required GameServerSettings GameServerSettings { get; set; }
	public required Gs2Settings        Gs2Settings        { get; set; }
}