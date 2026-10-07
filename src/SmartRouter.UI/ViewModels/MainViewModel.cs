using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using SmartRouter.Core.Diagnostics;
using SmartRouter.Core.Latency;
using SmartRouter.Core.Localization;
using SmartRouter.Core.Models;
using SmartRouter.Core.Rules;
using SmartRouter.Core.Services;

namespace SmartRouter.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly RouterService _router;
    private readonly LocalizationService _loc;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizationService Loc => _loc;

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusBrush));
                OnPropertyChanged(nameof(StartStopButtonText));
                OnPropertyChanged(nameof(StartStopButtonBrush));
            }
        }
    }

    public string StatusText => IsRunning ? _loc["StatusActive"] : _loc["StatusStopped"];
    public IBrush StatusBrush => IsRunning ? Brushes.LimeGreen : Brushes.Gray;
    public string StartStopButtonText => IsRunning ? _loc["Stop"] : _loc["Start"];
    public IBrush StartStopButtonBrush => IsRunning ? Brushes.Crimson : Brushes.DodgerBlue;

    public FlowDirection CurrentFlowDirection => _loc.IsPersian ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    // Metrics
    private long _totalQueries;
    public long TotalQueries
    {
        get => _totalQueries;
        set { _totalQueries = value; OnPropertyChanged(); }
    }

    private long _sanctionedQueries;
    public long SanctionedQueries
    {
        get => _sanctionedQueries;
        set { _sanctionedQueries = value; OnPropertyChanged(); }
    }

    private long _cleanQueries;
    public long CleanQueries
    {
        get => _cleanQueries;
        set { _cleanQueries = value; OnPropertyChanged(); }
    }

    private long _cacheHits;
    public long CacheHits
    {
        get => _cacheHits;
        set { _cacheHits = value; OnPropertyChanged(); }
    }

    private double _avgLatency;
    public double AvgLatency
    {
        get => _avgLatency;
        set { _avgLatency = value; OnPropertyChanged(); }
    }

    private string _notificationMessage = string.Empty;
    public string NotificationMessage
    {
        get => _notificationMessage;
        set { _notificationMessage = value; OnPropertyChanged(); }
    }

    private bool _isNotificationVisible;
    public bool IsNotificationVisible
    {
        get => _isNotificationVisible;
        set { _isNotificationVisible = value; OnPropertyChanged(); }
    }

    private string _newDomainInput = string.Empty;
    public string NewDomainInput
    {
        get => _newDomainInput;
        set { _newDomainInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<DnsProvider> Providers { get; } = [];
    public ObservableCollection<DiagnosticResult> DiagnosticResults { get; } = [];
    public ObservableCollection<QueryLogEntry> Logs { get; } = [];
    public ObservableCollection<DomainRule> Rules { get; } = [];

    private DnsProvider? _selectedAntiSanction;
    public DnsProvider? SelectedAntiSanction
    {
        get => _selectedAntiSanction;
        set
        {
            if (_selectedAntiSanction != value)
            {
                _selectedAntiSanction = value;
                if (value != null) _router.Config.SelectedAntiSanctionProviderId = value.Id;
                OnPropertyChanged();
            }
        }
    }

    private DnsProvider? _selectedClean;
    public DnsProvider? SelectedClean
    {
        get => _selectedClean;
        set
        {
            if (_selectedClean != value)
            {
                _selectedClean = value;
                if (value != null) _router.Config.SelectedCleanDnsProviderId = value.Id;
                OnPropertyChanged();
            }
        }
    }

    public MainViewModel()
    {
        _router = RouterService.Instance;
        _loc = LocalizationService.Instance;

        _loc.CurrentLanguage = _router.Config.Language;
        _loc.LanguageChanged += _ => RefreshAllLocalizations();

        _router.StatusChanged += isRunning =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsRunning = isRunning;
                UpdateMetrics();
            });
        };

        _router.QueryLogged += entry =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                Logs.Insert(0, entry);
                if (Logs.Count > 300) Logs.RemoveAt(Logs.Count - 1);
                UpdateMetrics();
            });
        };

        _router.NotificationTriggered += (msg, isError) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                NotificationMessage = msg;
                IsNotificationVisible = true;
            });
        };

        LoadProviders();
        LoadRules();
        InitDiagnostics();
    }

    private void RefreshAllLocalizations()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StartStopButtonText));
        OnPropertyChanged(nameof(CurrentFlowDirection));
        OnPropertyChanged(null); // refresh all bindings
        LoadProviders();
    }

    private void LoadProviders()
    {
        Providers.Clear();
        var all = ProviderRegistry.GetAllProviders(_router.Config);
        foreach (var p in all)
        {
            Providers.Add(p);
        }

        SelectedAntiSanction = Providers.FirstOrDefault(p => p.Id.Equals(_router.Config.SelectedAntiSanctionProviderId, StringComparison.OrdinalIgnoreCase))
                               ?? Providers.FirstOrDefault(p => p.IsAntiSanction);

        SelectedClean = Providers.FirstOrDefault(p => p.Id.Equals(_router.Config.SelectedCleanDnsProviderId, StringComparison.OrdinalIgnoreCase))
                        ?? Providers.FirstOrDefault(p => !p.IsAntiSanction);
    }

    public void LoadRules()
    {
        Rules.Clear();
        foreach (var r in _router.RuleEngine.AllRules.Take(200))
        {
            Rules.Add(r);
        }
    }

    private void InitDiagnostics()
    {
        DiagnosticResults.Clear();
        foreach (var t in SanctionDiagnosticRunner.GetDefaultTargets())
        {
            DiagnosticResults.Add(new DiagnosticResult
            {
                TargetId = t.Id,
                TargetName = t.Name,
                Url = t.Url,
                IsAccessible = false,
                MessageEn = "Ready to test",
                MessageFa = "آماده بررسی"
            });
        }
    }

    public void SetRoutingMode(RoutingMode mode)
    {
        _router.Config.Mode = mode;
        _router.SaveConfig();
        var modeName = mode switch
        {
            RoutingMode.SmartSplitDns => _loc["ModeSmartSplit"],
            RoutingMode.FullAntiSanction => _loc["ModeFullAntiSanction"],
            RoutingMode.FastestGlobal => _loc["ModeFastestGlobal"],
            _ => mode.ToString()
        };
        NotificationMessage = $"{_loc["ModeLabel"]}: {modeName}";
        IsNotificationVisible = true;
        OnPropertyChanged(nameof(StatusText));
    }

    public async Task ToggleStartStopAsync()
    {
        if (IsRunning)
        {
            await _router.StopAsync();
        }
        else
        {
            var success = await _router.StartAsync();
            if (!success)
            {
                NotificationMessage = _loc["ErrorPortInUse"];
                IsNotificationVisible = true;
            }
        }
    }

    public async Task BenchmarkAllPingsAsync()
    {
        NotificationMessage = _loc["TestingPing"];
        IsNotificationVisible = true;

        await _router.LatencyTester.BenchmarkAllAsync(Providers, (p, latency) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var idx = Providers.IndexOf(p);
                if (idx >= 0)
                {
                    Providers[idx] = p; // trigger notify
                }
            });
        });

        IsNotificationVisible = false;
    }

    public async Task RunDiagnosticsAsync()
    {
        NotificationMessage = _loc["RunDiagnostic"] + "...";
        IsNotificationVisible = true;

        await _router.DiagnosticRunner.RunAllAsync(res =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var existing = DiagnosticResults.FirstOrDefault(d => d.TargetId == res.TargetId);
                if (existing != null)
                {
                    var idx = DiagnosticResults.IndexOf(existing);
                    DiagnosticResults[idx] = res;
                }
            });
        });

        IsNotificationVisible = false;
    }

    public void AddCustomRule()
    {
        if (string.IsNullOrWhiteSpace(NewDomainInput)) return;

        var clean = NewDomainInput.Trim().ToLowerInvariant();
        var rule = new DomainRule
        {
            Pattern = clean,
            Category = RuleCategory.Custom,
            IsEnabled = true,
            IsWildcard = clean.Contains('*')
        };

        _router.Config.CustomRules.Add(rule);
        _router.RuleEngine.ReloadRules(_router.Config);
        _router.SaveConfig();

        Rules.Insert(0, rule);
        NewDomainInput = string.Empty;
    }

    public void RemoveRule(DomainRule rule)
    {
        if (rule == null) return;
        _router.Config.CustomRules.Remove(rule);
        Rules.Remove(rule);
        _router.RuleEngine.ReloadRules(_router.Config);
        _router.SaveConfig();
    }

    public void ClearLogs()
    {
        _router.ClearLogs();
        Logs.Clear();
        UpdateMetrics();
    }

    public void SwitchLanguage(string lang)
    {
        _loc.CurrentLanguage = lang;
        _router.Config.Language = lang;
        _router.SaveConfig();
    }

    public void SaveConfiguration()
    {
        _router.SaveConfig();
        NotificationMessage = _loc["ConfigSavedSuccess"];
        IsNotificationVisible = true;
    }

    private void UpdateMetrics()
    {
        TotalQueries = _router.DnsServer.TotalQueriesCount;
        SanctionedQueries = _router.DnsServer.SanctionedRoutedCount;
        CleanQueries = _router.DnsServer.CleanQueriesCount;
        CacheHits = _router.DnsServer.CacheHitsCount;
        AvgLatency = _router.DnsServer.AverageLatencyMs;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
