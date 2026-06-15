namespace GServ.Network;

public enum SessionLifecycle
{
    AwaitingLoginPrelude,
    LoginPreludeParsed,
    WaitingForServerListAuth,
    ServerListAuthAcceptedPreWorld,
    ReadyForWorldEntry,
    Authenticated,
    Rejected,
    Disconnecting,
    Disconnected
}
