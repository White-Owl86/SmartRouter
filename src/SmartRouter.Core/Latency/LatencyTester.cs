using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SmartRouter.Core.Models;

namespace SmartRouter.Core.Latency;

public class LatencyTester
{
    private static readonly byte[] DnsPingPacket =
    [
        0x12, 0x34, // ID
        0x01, 0x00, // Standard query, recursion desired
        0x00, 0x01, // Questions: 1
        0x00, 0x00, // Answers: 0
        0x00, 0x00, // Authority: 0
        0x00, 0x00, // Additional: 0
        // Name: "google.com"
        0x06, (byte)'g', (byte)'o', (byte)'o', (byte)'g', (byte)'l', (byte)'e',
        0x03, (byte)'c', (byte)'o', (byte)'m',
        0x00,       // End of name
        0x00, 0x01, // Type: A
        0x00, 0x01  // Class: IN
    ];

    public async Task<double> TestProviderLatencyAsync(DnsProvider provider, int timeoutMs = 2000, CancellationToken ct = default)
    {
        var primaryLatency = await MeasureSingleIpLatencyAsync(provider.PrimaryIp, timeoutMs, ct);
        if (primaryLatency >= 0)
        {
            provider.LatencyMs = primaryLatency;
            provider.IsReachable = true;
            provider.LastTested = DateTime.Now;
            return primaryLatency;
        }

        // Try secondary IP
        if (!string.IsNullOrWhiteSpace(provider.SecondaryIp))
        {
            var secondaryLatency = await MeasureSingleIpLatencyAsync(provider.SecondaryIp, timeoutMs, ct);
            if (secondaryLatency >= 0)
            {
                provider.LatencyMs = secondaryLatency;
                provider.IsReachable = true;
                provider.LastTested = DateTime.Now;
                return secondaryLatency;
            }
        }

        provider.LatencyMs = -1;
        provider.IsReachable = false;
        provider.LastTested = DateTime.Now;
        return -1;
    }

    public async Task<double> MeasureSingleIpLatencyAsync(string ipStr, int timeoutMs = 2000, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ipStr) || !IPAddress.TryParse(ipStr, out var ip))
            return -1;

        // 1. Try actual DNS UDP round-trip time (most accurate for DNS)
        try
        {
            using var udp = new UdpClient();
            udp.Client.ReceiveTimeout = timeoutMs;
            udp.Client.SendTimeout = timeoutMs;

            var sw = Stopwatch.StartNew();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            await udp.SendAsync(DnsPingPacket, new IPEndPoint(ip, 53), cts.Token);
            var receiveTask = udp.ReceiveAsync(cts.Token);
            var completed = await Task.WhenAny(receiveTask.AsTask(), Task.Delay(timeoutMs, cts.Token));

            if (completed == receiveTask.AsTask())
            {
                sw.Stop();
                var result = await receiveTask;
                if (result.Buffer.Length > 12)
                {
                    return Math.Round(sw.Elapsed.TotalMilliseconds, 1);
                }
            }
        }
        catch
        {
            // Fall through to TCP / ICMP
        }

        // 2. Try TCP Handshake on port 53
        try
        {
            using var tcp = new TcpClient();
            var sw = Stopwatch.StartNew();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            var connectTask = tcp.ConnectAsync(ip, 53, cts.Token);
            var completed = await Task.WhenAny(connectTask.AsTask(), Task.Delay(timeoutMs, cts.Token));

            if (completed == connectTask.AsTask() && tcp.Connected)
            {
                sw.Stop();
                return Math.Round(sw.Elapsed.TotalMilliseconds, 1);
            }
        }
        catch
        {
            // Fall through to ICMP
        }

        // 3. Fallback to ICMP Ping
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, timeoutMs);
            if (reply.Status == IPStatus.Success)
            {
                return reply.RoundtripTime;
            }
        }
        catch
        {
            // Ignore
        }

        return -1;
    }

    public async Task<List<DnsProvider>> BenchmarkAllAsync(
        IEnumerable<DnsProvider> providers,
        Action<DnsProvider, double>? onTested = null,
        CancellationToken ct = default)
    {
        var list = providers.ToList();
        var tasks = list.Select(async p =>
        {
            var latency = await TestProviderLatencyAsync(p, 2500, ct);
            onTested?.Invoke(p, latency);
            return p;
        });

        await Task.WhenAll(tasks);

        return list.OrderBy(p => p.IsReachable ? p.LatencyMs : double.MaxValue).ToList();
    }
}
