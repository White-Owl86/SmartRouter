namespace SmartRouter.Core.Models;

/// <summary>
/// Upstream DNS provider specification.
/// </summary>
public class DnsProvider
{
    public string Id { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameFa { get; set; } = string.Empty;
    public string PrimaryIp { get; set; } = string.Empty;
    public string SecondaryIp { get; set; } = string.Empty;
    public string? DohUrl { get; set; }
    public bool IsAntiSanction { get; set; }
    public bool IsGamingSpecialized { get; set; }
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionFa { get; set; } = string.Empty;

    // Runtime latency metrics
    public double LatencyMs { get; set; } = -1;
    public bool IsReachable { get; set; } = false;
    public DateTime LastTested { get; set; } = DateTime.MinValue;

    public string DisplayName(bool isPersian) => isPersian ? NameFa : NameEn;
    public string DisplayDescription(bool isPersian) => isPersian ? DescriptionFa : DescriptionEn;
}
