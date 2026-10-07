namespace SmartRouter.Core.Network;

public static class NetworkManagerFactory
{
    public static INetworkManager Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsNetworkManager();
        }
        if (OperatingSystem.IsLinux())
        {
            return new LinuxNetworkManager();
        }

        return new FallbackNetworkManager();
    }
}

public class FallbackNetworkManager : INetworkManager
{
    public bool IsSupported => false;
    public bool HasElevatedPrivileges => false;
    public string GetActiveAdapterName() => "Default";
    public Task<bool> ApplyDnsAsync(string primaryDns, string? secondaryDns = null) => Task.FromResult(false);
    public Task<bool> RestoreDnsAsync() => Task.FromResult(true);

    public string GetManualDnsInstructions(bool isPersian, string primaryDns) =>
        isPersian
            ? $"لطفاً دی‌ان‌اس سیستم‌عامل خود را به صورت دستی روی {primaryDns} تنظیم کنید."
            : $"Please manually configure your OS DNS server to {primaryDns}.";
}
