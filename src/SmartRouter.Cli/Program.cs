using System.Text;
using SmartRouter.Core.Diagnostics;
using SmartRouter.Core.Latency;
using SmartRouter.Core.Localization;
using SmartRouter.Core.Models;
using SmartRouter.Core.Network;
using SmartRouter.Core.Rules;
using SmartRouter.Core.Services;

namespace SmartRouter.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var loc = LocalizationService.Instance;
        var router = RouterService.Instance;

        // Check if --lang specified
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--lang", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                loc.CurrentLanguage = args[i + 1];
            }
        }

        var command = args.Length > 0 && !args[0].StartsWith("--") ? args[0].ToLowerInvariant() : "run";

        switch (command)
        {
            case "ping":
            case "benchmark":
                await RunPingBenchmarkAsync(router, loc);
                return 0;

            case "test":
            case "diag":
            case "diagnostics":
                await RunDiagnosticsAsync(router, loc);
                return 0;

            case "rules":
                ShowRules(router, loc);
                return 0;

            case "set-dns":
                await SetSystemDnsAsync(router, loc);
                return 0;

            case "restore-dns":
                await RestoreSystemDnsAsync(router, loc);
                return 0;

            case "help":
            case "--help":
            case "-h":
                ShowHelp(loc);
                return 0;

            case "run":
            case "start":
            default:
                await RunServerAsync(router, loc, args);
                return 0;
        }
    }

    private static void ShowBanner(LocalizationService loc)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=======================================================================");
        Console.WriteLine($"  {loc["AppTitle"]}");
        Console.WriteLine($"  {loc["AppSubtitle"]}");
        Console.WriteLine("=======================================================================");
        Console.ResetColor();
    }

    private static async Task RunServerAsync(RouterService router, LocalizationService loc, string[] args)
    {
        ShowBanner(loc);

        // Check privileges
        var netMgr = router.NetworkManager;
        if (!netMgr.HasElevatedPrivileges && router.Config.DnsPort == 53)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine($"[!] {loc["ErrorAdminRequiredWindows"]}");
            }
            else
            {
                Console.WriteLine($"[!] {loc["ErrorRootRequiredLinux"]}");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[*] {loc["StatusStarting"]}");
        Console.WriteLine($"[*] Mode: {router.Config.Mode}");
        Console.WriteLine($"[*] Anti-Sanction DNS: {router.Config.SelectedAntiSanctionProviderId}");
        Console.WriteLine($"[*] Clean Global DNS: {router.Config.SelectedCleanDnsProviderId}");
        Console.WriteLine($"[*] Listening on: {router.Config.ListenAddress}:{router.Config.DnsPort}");
        Console.ResetColor();

        router.QueryLogged += entry =>
        {
            var color = entry.IsSanctioned ? ConsoleColor.Magenta : ConsoleColor.DarkGray;
            if (entry.FromCache) color = ConsoleColor.DarkCyan;

            Console.ForegroundColor = color;
            var tag = entry.FromCache ? "[CACHE]" : (entry.IsSanctioned ? "[SANCTION-BYPASS]" : "[CLEAN]");
            Console.WriteLine($"{entry.Timestamp:HH:mm:ss} {tag,-17} {entry.Domain,-32} -> {entry.ResolvedIp,-16} ({entry.ResponseTimeMs} ms) [{entry.UpstreamUsed}]");
            Console.ResetColor();
        };

        var started = await router.StartAsync();
        if (!started)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[-] Failed to start Smart Router.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[+] {loc["StatusActive"]}");
        if (router.IsSystemDnsApplied)
        {
            Console.WriteLine($"[+] System DNS set to {router.Config.ListenAddress} on '{netMgr.GetActiveAdapterName()}'.");
        }
        else
        {
            Console.WriteLine($"[i] {netMgr.GetManualDnsInstructions(loc.IsPersian, router.Config.ListenAddress)}");
        }
        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine("Commands: [p] Ping benchmark | [t] Run diagnostics | [c] Clear console | [q] Quit");
        Console.WriteLine();

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        _ = Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.Q)
                    {
                        cts.Cancel();
                        break;
                    }
                    if (key == ConsoleKey.P)
                    {
                        _ = RunPingBenchmarkAsync(router, loc);
                    }
                    if (key == ConsoleKey.T)
                    {
                        _ = RunDiagnosticsAsync(router, loc);
                    }
                    if (key == ConsoleKey.C)
                    {
                        Console.Clear();
                        ShowBanner(loc);
                    }
                }
                Thread.Sleep(100);
            }
        });

        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(500);
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[*] {loc["StatusStopping"]}");
        await router.StopAsync();
        Console.WriteLine($"[+] {loc["DnsRestoredSuccess"]}");
        Console.ResetColor();
    }

    private static async Task RunPingBenchmarkAsync(RouterService router, LocalizationService loc)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"--- {loc["ProvidersTitle"]} ---");
        Console.ResetColor();

        var providers = ProviderRegistry.GetAllProviders(router.Config);
        var tester = router.LatencyTester;

        var results = await tester.BenchmarkAllAsync(providers, (p, latency) =>
        {
            var pingStr = latency >= 0 ? $"{latency,6:F1} ms" : "  TIMEOUT";
            var color = latency is >= 0 and < 60 ? ConsoleColor.Green :
                        latency is >= 60 and < 150 ? ConsoleColor.Yellow : ConsoleColor.Red;

            Console.ForegroundColor = color;
            var typeTag = p.IsAntiSanction ? "[Anti-Sanction]" : "[Global DNS]   ";
            Console.WriteLine($"  {typeTag} {p.DisplayName(loc.IsPersian),-24} {p.PrimaryIp,-16} -> {pingStr}");
            Console.ResetColor();
        });

        var fastestAnti = results.FirstOrDefault(p => p.IsAntiSanction && p.IsReachable);
        var fastestGlobal = results.FirstOrDefault(p => !p.IsAntiSanction && p.IsReachable);

        Console.WriteLine();
        if (fastestAnti != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[*] Fastest Anti-Sanction: {fastestAnti.DisplayName(loc.IsPersian)} ({fastestAnti.LatencyMs:F1} ms)");
        }
        if (fastestGlobal != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[*] Fastest Global Clean:  {fastestGlobal.DisplayName(loc.IsPersian)} ({fastestGlobal.LatencyMs:F1} ms)");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    private static async Task RunDiagnosticsAsync(RouterService router, LocalizationService loc)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"--- {loc["DiagnosticsTitle"]} ---");
        Console.WriteLine($"  {loc["DiagnosticsDesc"]}");
        Console.ResetColor();
        Console.WriteLine();

        var runner = router.DiagnosticRunner;
        await runner.RunAllAsync(res =>
        {
            var color = res.IsAccessible ? ConsoleColor.Green :
                        res.StatusCode == 403 ? ConsoleColor.Red : ConsoleColor.Yellow;

            var statusTag = res.IsAccessible ? "[UNBLOCKED]" :
                            res.StatusCode == 403 ? "[SANCTIONED]" : "[CHECK]";

            Console.ForegroundColor = color;
            Console.WriteLine($"  {statusTag,-12} {res.TargetName,-28} {res.DisplayMessage(loc.IsPersian)}");
            Console.ResetColor();
        });

        Console.WriteLine();
    }

    private static void ShowRules(RouterService router, LocalizationService loc)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"--- {loc["RulesTitle"]} ---");
        Console.ResetColor();

        var rules = router.RuleEngine.AllRules;
        var categories = rules.GroupBy(r => r.Category);

        foreach (var grp in categories)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[{grp.Key}] ({grp.Count()} rules):");
            Console.ResetColor();

            var sample = grp.Take(8).Select(r => r.Pattern);
            Console.WriteLine("  " + string.Join(", ", sample) + (grp.Count() > 8 ? "..." : ""));
        }

        Console.WriteLine();
        Console.WriteLine($"Total Active Rules: {rules.Count}");
        Console.WriteLine();
    }

    private static async Task SetSystemDnsAsync(RouterService router, LocalizationService loc)
    {
        var netMgr = router.NetworkManager;
        var success = await netMgr.ApplyDnsAsync(router.Config.ListenAddress);
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] System DNS set to {router.Config.ListenAddress} on '{netMgr.GetActiveAdapterName()}'.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[-] Failed to set DNS automatically. Administrator/Root privileges may be required.");
            Console.WriteLine(netMgr.GetManualDnsInstructions(loc.IsPersian, router.Config.ListenAddress));
        }
        Console.ResetColor();
    }

    private static async Task RestoreSystemDnsAsync(RouterService router, LocalizationService loc)
    {
        var netMgr = router.NetworkManager;
        var success = await netMgr.RestoreDnsAsync();
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] {loc["DnsRestoredSuccess"]}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[-] Failed to restore DNS.");
        }
        Console.ResetColor();
    }

    private static void ShowHelp(LocalizationService loc)
    {
        ShowBanner(loc);
        Console.WriteLine("Usage: smartrouter [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  run, start       Start the Smart Anti-Sanction & Latency Router (default)");
        Console.WriteLine("  ping, benchmark  Test and rank latency of all anti-sanction and global providers");
        Console.WriteLine("  test, diag       Run real-world connectivity checks (Docker, OpenAI, Android, Claude)");
        Console.WriteLine("  rules            Show active domain split-rules and categories");
        Console.WriteLine("  set-dns          Set system network adapter to 127.0.0.1");
        Console.WriteLine("  restore-dns      Restore original network adapter DNS");
        Console.WriteLine("  help             Show this help screen");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --lang [fa|en]   Set language to Persian (fa) or English (en)");
        Console.WriteLine();
    }
}
