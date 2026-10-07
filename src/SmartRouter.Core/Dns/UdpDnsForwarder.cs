using System.Net;
using System.Net.Sockets;

namespace SmartRouter.Core.Dns;

public class UdpDnsForwarder : IDnsForwarder
{
    private readonly IPEndPoint _primaryEndpoint;
    private readonly IPEndPoint? _secondaryEndpoint;
    private readonly int _timeoutMs;

    public string Name { get; }
    public string Endpoint => _primaryEndpoint.ToString();

    public UdpDnsForwarder(string name, string primaryIp, string? secondaryIp = null, int port = 53, int timeoutMs = 2500)
    {
        Name = name;
        _primaryEndpoint = new IPEndPoint(IPAddress.Parse(primaryIp), port);
        _secondaryEndpoint = !string.IsNullOrWhiteSpace(secondaryIp) && IPAddress.TryParse(secondaryIp, out var sIp)
            ? new IPEndPoint(sIp, port)
            : null;
        _timeoutMs = timeoutMs;
    }

    public async Task<byte[]?> ForwardAsync(byte[] queryBuffer, CancellationToken ct = default)
    {
        // Try Primary
        var response = await QueryEndpointAsync(_primaryEndpoint, queryBuffer, ct);
        if (response != null && response.Length > 0)
        {
            return response;
        }

        // Try Secondary if primary failed
        if (_secondaryEndpoint != null)
        {
            return await QueryEndpointAsync(_secondaryEndpoint, queryBuffer, ct);
        }

        return null;
    }

    private async Task<byte[]?> QueryEndpointAsync(IPEndPoint ep, byte[] queryBuffer, CancellationToken ct)
    {
        try
        {
            using var udp = new UdpClient();
            udp.Client.ReceiveTimeout = _timeoutMs;
            udp.Client.SendTimeout = _timeoutMs;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);

            await udp.SendAsync(queryBuffer, ep, cts.Token);
            var result = await udp.ReceiveAsync(cts.Token);
            return result.Buffer;
        }
        catch
        {
            return null;
        }
    }
}
