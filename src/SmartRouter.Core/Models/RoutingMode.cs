namespace SmartRouter.Core.Models;

/// <summary>
/// Operational routing mode of the Smart Router.
/// </summary>
public enum RoutingMode
{
    /// <summary>
    /// Routes sanctioned domains through anti-sanction providers, and clean domains through fast global DNS.
    /// </summary>
    SmartSplitDns,

    /// <summary>
    /// Routes all queries through the selected anti-sanction provider.
    /// </summary>
    FullAntiSanction,

    /// <summary>
    /// Routes all queries through the fastest tested global DNS (latency-optimized).
    /// </summary>
    FastestGlobal,

    /// <summary>
    /// Routes through custom user-defined upstream DNS.
    /// </summary>
    CustomUpstream
}
