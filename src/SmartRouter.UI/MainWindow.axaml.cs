using Avalonia.Controls;
using Avalonia.Interactivity;
using SmartRouter.Core.Models;
using SmartRouter.UI.ViewModels;

namespace SmartRouter.UI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;
    }

    private async void OnStartStopClicked(object? sender, RoutedEventArgs e)
    {
        await _vm.ToggleStartStopAsync();
    }

    private void OnSetFarsiClicked(object? sender, RoutedEventArgs e)
    {
        _vm.SwitchLanguage("fa");
    }

    private void OnSetEnglishClicked(object? sender, RoutedEventArgs e)
    {
        _vm.SwitchLanguage("en");
    }

    private async void OnBenchmarkPingsClicked(object? sender, RoutedEventArgs e)
    {
        await _vm.BenchmarkAllPingsAsync();
    }

    private async void OnRunDiagnosticsClicked(object? sender, RoutedEventArgs e)
    {
        await _vm.RunDiagnosticsAsync();
    }

    private void OnAddRuleClicked(object? sender, RoutedEventArgs e)
    {
        _vm.AddCustomRule();
    }

    private void OnDeleteRuleClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: DomainRule rule })
        {
            _vm.RemoveRule(rule);
        }
    }

    private void OnClearLogsClicked(object? sender, RoutedEventArgs e)
    {
        _vm.ClearLogs();
    }

    private void OnSelectProviderClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: DnsProvider provider })
        {
            if (provider.IsAntiSanction)
            {
                _vm.SelectedAntiSanction = provider;
            }
            else
            {
                _vm.SelectedClean = provider;
            }
            _vm.SaveConfiguration();
        }
    }

    private void OnSaveSettingsClicked(object? sender, RoutedEventArgs e)
    {
        _vm.SaveConfiguration();
    }

    private void OnExitClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnSetModeSmartSplitClicked(object? sender, RoutedEventArgs e)
    {
        _vm.SetRoutingMode(RoutingMode.SmartSplitDns);
    }

    private void OnSetModeFullAntiSanctionClicked(object? sender, RoutedEventArgs e)
    {
        _vm.SetRoutingMode(RoutingMode.FullAntiSanction);
    }

    private void OnSetModeFastestGlobalClicked(object? sender, RoutedEventArgs e)
    {
        _vm.SetRoutingMode(RoutingMode.FastestGlobal);
    }

    private void OnAboutClicked(object? sender, RoutedEventArgs e)
    {
        _vm.NotificationMessage = _vm.Loc["AboutMessage"];
        _vm.IsNotificationVisible = true;
    }
}