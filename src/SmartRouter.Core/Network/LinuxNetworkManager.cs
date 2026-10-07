using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SmartRouter.Core.Network;

public class LinuxNetworkManager : INetworkManager
{
    private const string BackupResolvFile = "/tmp/resolv.conf.smartrouter.bak";
    private string? _backedUpInterface;
    private bool _usedResolvectl = false;

    public bool IsSupported => OperatingSystem.IsLinux();

    public bool HasElevatedPrivileges
    {
        get
        {
            if (!OperatingSystem.IsLinux()) return false;
            try
            {
                return Environment.UserName == "root";
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
                          nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .OrderByDescending(nic => nic.GetIPProperties().GatewayAddresses.Count)
            .FirstOrDefault();

        return best?.Name ?? "eth0";
    }

    public async Task<bool> ApplyDnsAsync(string primaryDns, string? secondaryDns = null)
    {
        if (!IsSupported) return false;

        var iface = GetActiveAdapterName();
        _backedUpInterface = iface;

        // 1. Try resolvectl / systemd-resolved if available
        var checkResolvectl = await RunCommandAsync("which", "resolvectl");
        if (checkResolvectl.ExitCode == 0)
        {
            var dnsArgs = string.IsNullOrEmpty(secondaryDns)
                ? $"dns {iface} {primaryDns}"
                : $"dns {iface} {primaryDns} {secondaryDns}";

            var res = await RunCommandAsync("resolvectl", dnsArgs);
            if (res.ExitCode == 0)
            {
                _usedResolvectl = true;
                return true;
            }
        }

        // 2. Fallback to /etc/resolv.conf backup & write
        try
        {
            if (File.Exists("/etc/resolv.conf"))
            {
                File.Copy("/etc/resolv.conf", BackupResolvFile, overwrite: true);
                var content = $"nameserver {primaryDns}\n";
                if (!string.IsNullOrEmpty(secondaryDns)) content += $"nameserver {secondaryDns}\n";
                await File.WriteAllTextAsync("/etc/resolv.conf", content);
                return true;
            }
        }
        catch
        {
            // Permission denied or file system error
        }

        return false;
    }

    public async Task<bool> RestoreDnsAsync()
    {
        if (!IsSupported) return false;

        if (_usedResolvectl && !string.IsNullOrEmpty(_backedUpInterface))
        {
            var res = await RunCommandAsync("resolvectl", $"revert {_backedUpInterface}");
            _backedUpInterface = null;
            return res.ExitCode == 0;
        }

        if (File.Exists(BackupResolvFile))
        {
            try
            {
                File.Copy(BackupResolvFile, "/etc/resolv.conf", overwrite: true);
                File.Delete(BackupResolvFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<(int ExitCode, string Output)> RunCommandAsync(string fileName, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return (-1, "Process start failed");

            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();
            return (proc.ExitCode, output);
        }
        catch (Exception ex)
        {
            return (-1, ex.Message);
        }
    }

    public string GetManualDnsInstructions(bool isPersian, string primaryDns)
    {
        var iface = GetActiveAdapterName();
        if (isPersian)
        {
            return $"تنظیم دستی در لینوکس:\n" +
                   $"۱. اگر از systemd-resolved استفاده می‌کنید:\n" +
                   $"   sudo resolvectl dns {iface} {primaryDns}\n" +
                   $"۲. یا ویرایش مستقیم فایل resolv.conf:\n" +
                   $"   echo \"nameserver {primaryDns}\" | sudo tee /etc/resolv.conf\n" +
                   $"۳. برای بازگردانی به حالت اولیه:\n" +
                   $"   sudo resolvectl revert {iface}";
        }
        else
        {
            return $"Manual Linux Configuration:\n" +
                   $"1. Using systemd-resolved:\n" +
                   $"   sudo resolvectl dns {iface} {primaryDns}\n" +
                   $"2. Or direct /etc/resolv.conf modification:\n" +
                   $"   echo \"nameserver {primaryDns}\" | sudo tee /etc/resolv.conf\n" +
                   $"3. To revert:\n" +
                   $"   sudo resolvectl revert {iface}";
        }
    }
}
