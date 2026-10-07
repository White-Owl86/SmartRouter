using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using SmartRouter.Core.Latency;
using SmartRouter.Core.Models;
using SmartRouter.Core.Rules;

namespace SmartRouter.Core.Dns;

public class SmartDnsServer
{
    private readonly AppConfig _config;
    private readonly DomainRuleEngine _ruleEngine;
    private readonly DnsCache _cache = new();

    private UdpClient? _udpListener;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;

    private IDnsForwarder _antiSanctionForwarder = null!;
    private IDnsForwarder _cleanForwarder = null!;

    public bool IsRunning { get; private set; }

    // Metrics
    public long TotalQueriesCount { get; private set; }
    public long SanctionedRoutedCount { get; private set; }
    public long CleanQueriesCount { get; private set; }
    public long CacheHitsCount { get; private set; }
    public double AverageLatencyMs { get; private set; }

    public event Action<QueryLogEntry>? OnQueryHandled;
    public event Action<string, Exception?>? OnError;

    public SmartDnsServer(AppConfig config, DomainRuleEngine ruleEngine)
    {
        _config = config;
        _ruleEngine = ruleEngine;
        ConfigureForwarders();
    }

    public void ConfigureForwarders()
    {
        var allProviders = ProviderRegistry.GetAllProviders(_config);

        // Configure Anti-Sanction forwarder
        var antiProvider = allProviders.FirstOrDefault(p => p.Id.Equals(_config.SelectedAntiSanctionProviderId, StringComparison.OrdinalIgnoreCase))
                           ?? allProviders.First(p => p.IsAntiSanction);

        _antiSanctionForwarder = new UdpDnsForwarder(
            antiProvider.DisplayName(false),
            antiProvider.PrimaryIp,
            antiProvider.SecondaryIp
        );

        // Configure Clean forwarder
        var cleanProvider = allProviders.FirstOrDefault(p => p.Id.Equals(_config.SelectedCleanDnsProviderId, StringComparison.OrdinalIgnoreCase))
                            ?? allProviders.First(p => !p.IsAntiSanction);

        if (!string.IsNullOrEmpty(cleanProvider.DohUrl))
        {
            _cleanForwarder = new DohDnsForwarder(cleanProvider.DisplayName(false), cleanProvider.DohUrl);
        }
        else
        {
            _cleanForwarder = new UdpDnsForwarder(
                cleanProvider.DisplayName(false),
                cleanProvider.PrimaryIp,
                cleanProvider.SecondaryIp
            );
        }
    }

    public void Start()
    {
        if (IsRunning) return;

        ConfigureForwarders();

        var listenIp = IPAddress.Parse(_config.ListenAddress);
        var endpoint = new IPEndPoint(listenIp, _config.DnsPort);

        try
        {
            _udpListener = new UdpClient(endpoint);
            _cts = new CancellationTokenSource();
            IsRunning = true;
            _listenTask = Task.Run(() => ListenUdpLoopAsync(_cts.Token));
        }
        catch (SocketException ex)
        {
            IsRunning = false;
            OnError?.Invoke($"Failed to bind to {endpoint}. Port {_config.DnsPort} might be in use or requires Administrator/Root privileges.", ex);
            throw;
        }
    }

    public void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        try
        {
            _cts?.Cancel();
            _udpListener?.Close();
            _udpListener?.Dispose();
        }
        catch
        {
            // Ignore during shutdown
        }
        finally
        {
            _udpListener = null;
            _cts = null;
        }
    }

    private async Task ListenUdpLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _udpListener != null)
        {
            try
            {
                var result = await _udpListener.ReceiveAsync(ct);
                _ = Task.Run(() => HandleQueryAsync(result.Buffer, result.RemoteEndPoint, ct), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!ct.IsCancellationRequested)
                {
                    OnError?.Invoke("UDP receive error in DNS loop", ex);
                }
            }
        }
    }

    private async Task HandleQueryAsync(byte[] requestBuffer, IPEndPoint clientEndpoint, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        TotalQueriesCount++;

        var packet = DnsPacket.Parse(requestBuffer);
        var domain = packet?.Questions.FirstOrDefault()?.Name ?? "unknown";
        var qtype = packet?.Questions.FirstOrDefault()?.Type ?? DnsRecordType.A;
        var queryId = packet?.Id ?? 0;

        // 1. Check Cache
        if (_config.EnableDnsCache && _cache.TryGet(domain, qtype, queryId, out var cachedBytes, out var cachedIp))
        {
            CacheHitsCount++;
            await SendResponseAsync(cachedBytes!, clientEndpoint, ct);
            sw.Stop();

            OnQueryHandled?.Invoke(new QueryLogEntry
            {
                Domain = domain,
                RecordType = qtype.ToString(),
                IsSanctioned = false,
                FromCache = true,
                UpstreamUsed = "Local Cache",
                ResponseTimeMs = Math.Round(sw.Elapsed.TotalMilliseconds, 1),
                ResolvedIp = cachedIp,
                Status = "Cached"
            });
            return;
        }

        // 2. Determine Route
        var ruleMatch = _ruleEngine.Match(domain);
        var routeSanctioned = _config.Mode switch
        {
            RoutingMode.FullAntiSanction => true,
            RoutingMode.FastestGlobal => false,
            _ => ruleMatch.IsSanctioned
        };

        if (routeSanctioned)
        {
            SanctionedRoutedCount++;
        }
        else
        {
            CleanQueriesCount++;
        }

        var activeForwarder = routeSanctioned ? _antiSanctionForwarder : _cleanForwarder;

        // 3. Forward Query
        byte[]? responseBytes = null;
        try
        {
            responseBytes = await activeForwarder.ForwardAsync(requestBuffer, ct);
        }
        catch
        {
            // Upstream failed, attempt fallback
        }

        // Fallback to other forwarder if primary returned null
        if (responseBytes == null || responseBytes.Length == 0)
        {
            var fallback = routeSanctioned ? _cleanForwarder : _antiSanctionForwarder;
            try
            {
                responseBytes = await fallback.ForwardAsync(requestBuffer, ct);
            }
            catch
            {
                // Fallback also failed
            }
        }

        sw.Stop();
        var elapsedMs = Math.Round(sw.Elapsed.TotalMilliseconds, 1);
        UpdateLatencyMetric(elapsedMs);

        var resolvedIp = string.Empty;
        var status = "Failed";

        if (responseBytes != null && responseBytes.Length > 0)
        {
            // Ensure response ID matches client query ID
            DnsPacket.RewriteId(responseBytes, queryId);
            await SendResponseAsync(responseBytes, clientEndpoint, ct);

            status = "Resolved";
            var parsedResp = DnsPacket.Parse(responseBytes);
            var firstIpRecord = parsedResp?.Answers.FirstOrDefault(a => a.Type == DnsRecordType.A || a.Type == DnsRecordType.AAAA);
            if (firstIpRecord != null)
            {
                resolvedIp = firstIpRecord.AsIpAddress()?.ToString() ?? string.Empty;
            }

            // Cache response
            if (_config.EnableDnsCache && !string.IsNullOrEmpty(resolvedIp))
            {
                _cache.Set(domain, qtype, responseBytes, _config.DnsCacheTtlSeconds, resolvedIp);
            }
        }

        OnQueryHandled?.Invoke(new QueryLogEntry
        {
            Domain = domain,
            RecordType = qtype.ToString(),
            IsSanctioned = routeSanctioned,
            Category = ruleMatch.Category,
            FromCache = false,
            UpstreamUsed = activeForwarder.Name,
            ResponseTimeMs = elapsedMs,
            ResolvedIp = resolvedIp,
            Status = status
        });
    }

    private async Task SendResponseAsync(byte[] responseBytes, IPEndPoint clientEndpoint, CancellationToken ct)
    {
        if (_udpListener != null)
        {
            try
            {
                await _udpListener.SendAsync(responseBytes, clientEndpoint, ct);
            }
            catch
            {
                // Client may have closed socket
            }
        }
    }

    private void UpdateLatencyMetric(double currentMs)
    {
        if (AverageLatencyMs <= 0) AverageLatencyMs = currentMs;
        else AverageLatencyMs = Math.Round((AverageLatencyMs * 0.9) + (currentMs * 0.1), 1);
    }
}
