using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Principal;

namespace SmartRouter.Core.Network;

public class WindowsNetworkManager : INetworkManager
{
    private string? _backedUpAdapter;
    private bool _wasDhcp = true;
    private List<string> _originalDnsList = [];

    public bool IsSupported => OperatingSystem.IsWindows();

    public bool HasElevatedPrivileges
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return false;
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }

    public string GetActiveAdapterName()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        var best = interfaces
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                          nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                          nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
            .OrderByDescending(nic => nic.GetIPProperties().GatewayAddresses.Count)
            .FirstOrDefault();

        return best?.Name ?? "Wi-Fi";
    }

    public async Task<bool> ApplyDnsAsync(string primaryDns, string? secondaryDns = null)
    {
        if (!IsSupported) return false;

        var adapter = GetActiveAdapterName();
        _backedUpAdapter = adapter;
        BackupCurrentDns(adapter);

        var args = string.IsNullOrEmpty(secondaryDns)
            ? $"Set-DnsClientServerAddress -InterfaceAlias '{adapter}' -ServerAddresses ('{primaryDns}')"
            : $"Set-DnsClientServerAddress -InterfaceAlias '{adapter}' -ServerAddresses ('{primaryDns}','{secondaryDns}')";

        var result = await RunPowerShellAsync(args);
        return result.ExitCode == 0;
    }

    public async Task<bool> RestoreDnsAsync()
    {
        if (!IsSupported || string.IsNullOrEmpty(_backedUpAdapter)) return false;

        string script;
        if (_wasDhcp || _originalDnsList.Count == 0)
        {
            script = $"Set-DnsClientServerAddress -InterfaceAlias '{_backedUpAdapter}' -ResetServerAddresses";
        }
        else
        {
            var ips = string.Join("','", _originalDnsList);
            script = $"Set-DnsClientServerAddress -InterfaceAlias '{_backedUpAdapter}' -ServerAddresses ('{ips}')";
        }

        var result = await RunPowerShellAsync(script);
        _backedUpAdapter = null;
        return result.ExitCode == 0;
    }

    private void BackupCurrentDns(string adapterName)
    {
        _originalDnsList.Clear();
        _wasDhcp = true;

        var nic = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n => n.Name.Equals(adapterName, StringComparison.OrdinalIgnoreCase));

        if (nic != null)
        {
            var ipProps = nic.GetIPProperties();
            var dnsServers = ipProps.DnsAddresses
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToList();

            if (dnsServers.Count > 0 && !dnsServers.Contains("127.0.0.1"))
            {
                _originalDnsList.AddRange(dnsServers);
                _wasDhcp = false;
            }
        }
    }

    private static async Task<(int ExitCode, string Output)> RunPowerShellAsync(string script)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return (-1, "Process failed to start");

            var output = await proc.StandardOutput.ReadToEndAsync();
            var error = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            return (proc.ExitCode, string.IsNullOrWhiteSpace(error) ? output : error);
        }
        catch (Exception ex)
        {
            return (-1, ex.Message);
        }
    }

    public string GetManualDnsInstructions(bool isPersian, string primaryDns)
    {
        var adapter = GetActiveAdapterName();
        if (isPersian)
        {
            return $"تنظیم دستی در ویندوز:\n" +
                   $"۱. کلیدهای Win+R را زده و عبارت ncpa.cpl را اجرا کنید.\n" +
                   $"۲. روی کارت شبکه «{adapter}» راست کلیک کرده و Properties را بزنید.\n" +
                   $"۳. روی Internet Protocol Version 4 (TCP/IPv4) دوبار کلیک کنید.\n" +
                   $"۴. گزینه Use the following DNS server addresses را انتخاب کرده و مقدار Preferred DNS را روی {primaryDns} بگذارید.\n" +
                   $"۵. هنگام اتمام کار برای بازگردانی به حالت قبل، مجدداً گزینه Obtain DNS server address automatically را انتخاب فرمایید.";
        }
        else
        {
            return $"Manual Windows Configuration:\n" +
                   $"1. Press Win+R, type 'ncpa.cpl' and hit Enter.\n" +
                   $"2. Right click on '{adapter}' and select Properties.\n" +
                   $"3. Double click on 'Internet Protocol Version 4 (TCP/IPv4)'.\n" +
                   $"4. Select 'Use the following DNS server addresses' and enter {primaryDns} as Preferred DNS.\n" +
                   $"5. When finished, switch back to 'Obtain DNS server address automatically'.";
        }
    }
}
