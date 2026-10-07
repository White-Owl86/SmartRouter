using SmartRouter.Core.Models;

namespace SmartRouter.Core.Rules;

public class RuleMatchResult
{
    public bool IsMatched { get; set; }
    public bool IsSanctioned { get; set; }
    public RuleCategory? Category { get; set; }
    public string? MatchedPattern { get; set; }

    public static RuleMatchResult Clean() => new()
    {
        IsMatched = false,
        IsSanctioned = false,
        Category = null,
        MatchedPattern = null
    };
}

public class DomainRuleEngine
{
    private readonly List<DomainRule> _allRules = [];
    private readonly HashSet<string> _exactSanctionedDomains = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _exactBypassDomains = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<DomainRule> _wildcardRules = [];

    public IReadOnlyList<DomainRule> AllRules => _allRules;

    public DomainRuleEngine(AppConfig config)
    {
        ReloadRules(config);
    }

    public void ReloadRules(AppConfig config)
    {
        _allRules.Clear();
        _exactSanctionedDomains.Clear();
        _exactBypassDomains.Clear();
        _wildcardRules.Clear();

        // 1. Load default rules
        var defaults = DefaultRules.GetDefaultRules();
        foreach (var rule in defaults)
        {
            if (IsCategoryActive(rule.Category, config))
            {
                _allRules.Add(rule);
            }
        }

        // 2. Load custom user rules
        if (config.CustomRules != null)
        {
            foreach (var customRule in config.CustomRules.Where(r => r.IsEnabled))
            {
                _allRules.Add(customRule);
            }
        }

        // 3. Index rules for fast matching
        foreach (var rule in _allRules.Where(r => r.IsEnabled))
        {
            var pat = rule.Pattern.Trim().ToLowerInvariant();
            if (rule.Category == RuleCategory.Bypass)
            {
                if (pat.StartsWith("*."))
                {
                    _wildcardRules.Add(rule);
                }
                else
                {
                    _exactBypassDomains.Add(pat);
                }
            }
            else
            {
                if (pat.StartsWith("*."))
                {
                    _wildcardRules.Add(rule);
                }
                else
                {
                    _exactSanctionedDomains.Add(pat);
                    // Also implicitly treat as suffix rule:
                    _wildcardRules.Add(new DomainRule
                    {
                        Pattern = "*." + pat,
                        Category = rule.Category,
                        IsEnabled = true,
                        IsWildcard = true
                    });
                }
            }
        }
    }

    private static bool IsCategoryActive(RuleCategory category, AppConfig config) => category switch
    {
        RuleCategory.Developer => config.DeveloperCategoryEnabled,
        RuleCategory.AiCloud => config.AiCloudCategoryEnabled,
        RuleCategory.Gaming => config.GamingCategoryEnabled,
        RuleCategory.Bypass => config.BypassLocalTraffic,
        RuleCategory.Custom => true,
        _ => true
    };

    public RuleMatchResult Match(string rawDomain)
    {
        if (string.IsNullOrWhiteSpace(rawDomain))
            return RuleMatchResult.Clean();

        var domain = rawDomain.Trim().TrimEnd('.').ToLowerInvariant();

        // 1. Check bypass first
        if (_exactBypassDomains.Contains(domain))
        {
            return new RuleMatchResult
            {
                IsMatched = true,
                IsSanctioned = false,
                Category = RuleCategory.Bypass,
                MatchedPattern = domain
            };
        }

        // 2. Check exact sanctioned domain match
        if (_exactSanctionedDomains.Contains(domain))
        {
            var rule = _allRules.FirstOrDefault(r => r.Pattern.Equals(domain, StringComparison.OrdinalIgnoreCase));
            return new RuleMatchResult
            {
                IsMatched = true,
                IsSanctioned = true,
                Category = rule?.Category ?? RuleCategory.Developer,
                MatchedPattern = domain
            };
        }

        // 3. Suffix / Wildcard check
        foreach (var w in _wildcardRules)
        {
            var pattern = w.Pattern.Trim().ToLowerInvariant();
            if (pattern.StartsWith("*."))
            {
                var suffix = pattern[1..]; // e.g. ".docker.io"
                if (domain.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    var isBypass = w.Category == RuleCategory.Bypass;
                    return new RuleMatchResult
                    {
                        IsMatched = true,
                        IsSanctioned = !isBypass,
                        Category = w.Category,
                        MatchedPattern = pattern
                    };
                }
            }
        }

        return RuleMatchResult.Clean();
    }
}
