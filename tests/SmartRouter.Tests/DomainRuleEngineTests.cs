using SmartRouter.Core.Models;
using SmartRouter.Core.Rules;
using Xunit;

namespace SmartRouter.Tests;

public class DomainRuleEngineTests
{
    [Theory]
    [InlineData("docker.io", true, RuleCategory.Developer)]
    [InlineData("registry-1.docker.io", true, RuleCategory.Developer)]
    [InlineData("api.openai.com", true, RuleCategory.AiCloud)]
    [InlineData("chatgpt.com", true, RuleCategory.AiCloud)]
    [InlineData("steampowered.com", true, RuleCategory.Gaming)]
    [InlineData("steamcommunity.com", true, RuleCategory.Gaming)]
    [InlineData("developer.android.com", true, RuleCategory.Developer)]
    [InlineData("dl.google.com", true, RuleCategory.Developer)]
    [InlineData("varzesh3.com", false, null)]
    [InlineData("digikala.com", false, null)]
    [InlineData("test.ir", false, RuleCategory.Bypass)]
    public void Match_StandardDomains_CorrectlyIdentifiesSanctionsAndBypass(
        string domain,
        bool expectedSanctioned,
        RuleCategory? expectedCategory)
    {
        var config = new AppConfig();
        var engine = new DomainRuleEngine(config);

        var result = engine.Match(domain);

        Assert.Equal(expectedSanctioned, result.IsSanctioned);
        if (expectedCategory.HasValue)
        {
            Assert.Equal(expectedCategory.Value, result.Category);
        }
    }

    [Fact]
    public void CustomRule_AddsAndMatchesCorrectly()
    {
        var config = new AppConfig();
        config.CustomRules.Add(new DomainRule
        {
            Pattern = "*.mycustomblockedservice.com",
            Category = RuleCategory.Custom,
            IsEnabled = true,
            IsWildcard = true
        });

        var engine = new DomainRuleEngine(config);
        var match = engine.Match("sub.mycustomblockedservice.com");

        Assert.True(match.IsMatched);
        Assert.True(match.IsSanctioned);
        Assert.Equal(RuleCategory.Custom, match.Category);
    }
}
