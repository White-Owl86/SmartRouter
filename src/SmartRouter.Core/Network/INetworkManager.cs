namespace SmartRouter.Core.Network;

public interface INetworkManager
{
    bool IsSupported { get; }
    bool HasElevatedPrivileges { get; }
    string GetActiveAdapterName();
    Task<bool> ApplyDnsAsync(string primaryDns, string? secondaryDns = null);
    Task<bool> RestoreDnsAsync();
    string GetManualDnsInstructions(bool isPersian, string primaryDns);
}
