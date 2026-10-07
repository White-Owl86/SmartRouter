namespace SmartRouter.Core.Models;

/// <summary>
/// Live log entry representing an intercepted DNS resolution event.
/// </summary>
public class QueryLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Domain { get; set; } = string.Empty;
    public string RecordType { get; set; } = "A";
    public bool IsSanctioned { get; set; }
    public RuleCategory? Category { get; set; }
    public string UpstreamUsed { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; }
    public bool FromCache { get; set; }
    public string ResolvedIp { get; set; } = string.Empty;
    public string Status { get; set; } = "OK";
}
