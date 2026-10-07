using System.Collections.Concurrent;
using SmartRouter.Core.Diagnostics;
using SmartRouter.Core.Dns;
using SmartRouter.Core.Latency;
using SmartRouter.Core.Models;
using SmartRouter.Core.Network;
using SmartRouter.Core.Rules;

namespace SmartRouter.Core.Services;

public class RouterService : IDisposable
{
    private static readonly Lazy<RouterService> _instance = new(() => new RouterService());
    public static RouterService Instance => _instance.Value;

    private readonly INetworkManager _networkManager;
    private readonly LatencyTester _latencyTester = new();
    private readonly SanctionDiagnosticRunner _diagnosticRunner = new();
    private readonly ConcurrentQueue<QueryLogEntry> _queryLogs = new();
    private const int MaxLogEntries = 1000;

    public AppConfig Config { get; private set; }
    public DomainRuleEngine RuleEngine { get; private set; }
    public SmartDnsServer DnsServer { get; private set; }
    public INetworkManager NetworkManager => _networkManager;
    public LatencyTester LatencyTester => _latencyTester;
    public SanctionDiagnosticRunner DiagnosticRunner => _diagnosticRunner;

    public bool IsRunning => DnsServer.IsRunning;
    public bool IsSystemDnsApplied { get; private set; }

    public IReadOnlyCollection<QueryLogEntry> QueryLogs => _queryLogs;

    public event Action<bool>? StatusChanged;
    public event Action<QueryLogEntry>? QueryLogged;
    public event Action<string, bool>? NotificationTriggered; // message, isError

    public RouterService()
    {
        var configPath = GetConfigFilePath();
        Config = AppConfig.Load(configPath);
        RuleEngine = new DomainRuleEngine(Config);
        DnsServer = new SmartDnsServer(Config, RuleEngine);
        _networkManager = NetworkManagerFactory.Create();

        DnsServer.OnQueryHandled += HandleDnsQueryLogged;
        DnsServer.OnError += HandleDnsError;

        // Auto-revert DNS on unexpected process exit
        AppDomain.CurrentDomain.ProcessExit += (_, _) => SafeShutdown();
        Console.CancelKeyPress += (_, _) => SafeShutdown();
    }

    public static string GetConfigFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "SmartRouter");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, "config.json");
    }

    public void SaveConfig()
    {
        Config.Save(GetConfigFilePath());
    }

    public async Task<bool> StartAsync()
    {
        if (IsRunning) return true;

        try
        {
            RuleEngine.ReloadRules(Config);
            DnsServer.ConfigureForwarders();
            DnsServer.Start();

            if (Config.AutoApplySystemDns)
            {
                var applied = await _networkManager.ApplyDnsAsync(Config.ListenAddress);
                IsSystemDnsApplied = applied;
            }

            StatusChanged?.Invoke(true);
            return true;
        }
        catch (Exception ex)
        {
            NotificationTriggered?.Invoke(ex.Message, true);
            await StopAsync();
            return false;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            if (IsSystemDnsApplied)
            {
                await _networkManager.RestoreDnsAsync();
                IsSystemDnsApplied = false;
            }
        }
        catch (Exception ex)
        {
            NotificationTriggered?.Invoke($"Failed to restore DNS: {ex.Message}", true);
        }

        try
        {
            DnsServer.Stop();
        }
        finally
        {
            StatusChanged?.Invoke(false);
        }
    }

    public void ClearLogs()
    {
        _queryLogs.Clear();
    }

    private void HandleDnsQueryLogged(QueryLogEntry entry)
    {
        _queryLogs.Enqueue(entry);
        while (_queryLogs.Count > MaxLogEntries)
        {
            _queryLogs.TryDequeue(out _);
        }
        QueryLogged?.Invoke(entry);
    }

    private void HandleDnsError(string msg, Exception? ex)
    {
        NotificationTriggered?.Invoke(msg, true);
    }

    private void SafeShutdown()
    {
        try
        {
            if (IsSystemDnsApplied)
            {
                _networkManager.RestoreDnsAsync().GetAwaiter().GetResult();
                IsSystemDnsApplied = false;
            }
            DnsServer.Stop();
        }
        catch
        {
            // Silent during process kill
        }
    }

    public void Dispose()
    {
        SafeShutdown();
        GC.SuppressFinalize(this);
    }
}
