namespace SmartRouter.Core.Models;

/// <summary>
/// Represents a routing rule for domain matching.
/// </summary>
public class DomainRule
{
    public string Pattern { get; set; } = string.Empty;
    public RuleCategory Category { get; set; } = RuleCategory.Custom;
    public bool IsWildcard { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public string? Description { get; set; }
}
