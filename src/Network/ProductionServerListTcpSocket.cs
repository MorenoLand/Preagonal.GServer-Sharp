using System.Net;
using System.Net.Sockets;
using GServ.Protocol;

namespace GServ.Network;

public sealed class ProductionServerListTcpSocket : IProductionServerListSocket, IDisposable
{
    private readonly GraalFileQueue _queue = new();
    private string _host = "";
    private int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;

    public bool IsConnected => _client?.Connected == true;

    public string LocalIp =>
        _client?.Client.LocalEndPoint is IPEndPoint endpoint
            ? endpoint.Address.ToString()
            : string.Empty;

    public bool Initialize(string host, string port)
    {
        if (!int.TryParse(port, out var parsedPort) || parsedPort < IPEndPoint.MinPort || parsedPort > IPEndPoint.MaxPort)
            return false;

        _host = host;
        _port = parsedPort;
        return true;
    }

    public bool Connect()
    {
        if (IsConnected)
            return true;

        try
        {
            _client = new TcpClient();
            _client.NoDelay = true;
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            return true;
        }
        catch (SocketException)
        {
            DisposeClient();
            return false;
        }
        catch (IOException)
        {
            DisposeClient();
            return false;
        }
    }

    public void Register()
    {
    }

    public void ClearOutgoingBuffers()
    {
    }

    public void SetCodec(EncryptionGeneration generation, byte key)
    {
        _queue.SetCodec(generation, key);
    }

    public void SendPacket(byte[] packetBody, bool sendNow = false)
    {
        if (_stream is null)
            throw new InvalidOperationException("Server-list socket must be connected before sending packets.");

        var packet = packetBody.Length > 0 && packetBody[^1] == (byte)'\n'
            ? packetBody
            : [..packetBody, (byte)'\n'];
        _queue.AddRawPacket(packet);
        var bytes = _queue.FlushSocket(forceSendFiles: sendNow);
        if (bytes.Length == 0)
            return;

        _stream.Write(bytes);
    }

    public void Dispose()
    {
        DisposeClient();
    }

    private void DisposeClient()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }
}
