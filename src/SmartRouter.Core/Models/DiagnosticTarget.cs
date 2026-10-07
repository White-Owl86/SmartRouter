namespace SmartRouter.Core.Models;

public class DiagnosticTarget
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public RuleCategory Category { get; set; }
    public string ExpectedStatusHint { get; set; } = string.Empty;
}

public class DiagnosticResult
{
    public string TargetId { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsAccessible { get; set; }
    public int StatusCode { get; set; }
    public double LatencyMs { get; set; }
    public string MessageEn { get; set; } = string.Empty;
    public string MessageFa { get; set; } = string.Empty;
    public DateTime TestedAt { get; set; } = DateTime.Now;

    public string DisplayMessage(bool isPersian) => isPersian ? MessageFa : MessageEn;
}
