using SmartRouter.Core.Models;

namespace SmartRouter.Core.Rules;

public static class DefaultRules
{
    public static List<DomainRule> GetDefaultRules()
    {
        var rules = new List<DomainRule>();

        // Developer Repositories and SDKs
        AddRules(rules, RuleCategory.Developer,
            "docker.io", "*.docker.io", "registry-1.docker.io", "auth.docker.io", "hub.docker.com", "docker.com", "*.docker.com",
            "developer.android.com", "dl.google.com", "android.googlesource.com", "source.android.com", "pub.dev", "flutter.dev", "dart.dev",
            "gradle.org", "*.gradle.org", "services.gradle.org",
            "oracle.com", "*.oracle.com", "download.oracle.com", "java.com",
            "jetbrains.com", "*.jetbrains.com", "plugins.jetbrains.com", "download.jetbrains.com",
            "unity.com", "*.unity.com", "unity3d.com", "*.unity3d.com", "license.unity3d.com", "api.unity.com", "core.cloud.unity3d.com",
            "unrealengine.com", "*.unrealengine.com",
            "npmjs.org", "*.npmjs.org", "npmjs.com", "*.npmjs.com", "registry.npmjs.org",
            "repo1.maven.org", "maven.org",
            "anaconda.com", "*.anaconda.com", "repo.anaconda.com",
            "kaggle.com", "*.kaggle.com",
            "postman.com", "*.postman.com",
            "sentry.io", "*.sentry.io",
            "gitlab.com", "*.gitlab.com"
        );

        // AI & Cloud Services
        AddRules(rules, RuleCategory.AiCloud,
            "openai.com", "*.openai.com", "api.openai.com", "chatgpt.com", "*.chatgpt.com", "platform.openai.com", "cdn.oaistatic.com",
            "anthropic.com", "*.anthropic.com", "api.anthropic.com", "claude.ai", "*.claude.ai",
            "huggingface.co", "*.huggingface.co", "hf.co",
            "supabase.com", "*.supabase.com", "supabase.co", "*.supabase.co",
            "vercel.com", "*.vercel.com", "vercel.app", "*.vercel.app",
            "render.com", "*.render.com", "railway.app", "*.railway.app",
            "midjourney.com", "*.midjourney.com", "cohere.ai", "*.cohere.ai",
            "cloud.google.com", "firebase.google.com", "firebaseio.com", "*.firebaseio.com",
            "aws.amazon.com", "*.amazonaws.com"
        );

        // Gaming Platforms
        AddRules(rules, RuleCategory.Gaming,
            "steampowered.com", "*.steampowered.com", "steamcommunity.com", "*.steamcommunity.com", "steamstatic.com", "*.steamstatic.com", "steamcontent.com",
            "epicgames.com", "*.epicgames.com", "fortnite.com", "*.fortnite.com",
            "riotgames.com", "*.riotgames.com", "leagueoflegends.com", "pvp.net", "*.pvp.net",
            "blizzard.com", "*.blizzard.com", "battle.net", "*.battle.net",
            "ea.com", "*.ea.com", "origin.com", "*.origin.com",
            "ubisoft.com", "*.ubisoft.com",
            "playstation.com", "*.playstation.com", "playstation.net", "*.playstation.net",
            "xbox.com", "*.xbox.com", "xboxlive.com", "*.xboxlive.com",
            "discord.com", "*.discord.com", "discord.gg", "discordapp.com", "*.discordapp.com"
        );

        // Bypass / Direct
        AddRules(rules, RuleCategory.Bypass,
            "*.ir", "localhost", "*.local", "*.lan", "*.internal"
        );

        return rules;
    }

    private static void AddRules(List<DomainRule> list, RuleCategory category, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            list.Add(new DomainRule
            {
                Pattern = pattern.Trim().ToLowerInvariant(),
                Category = category,
                IsWildcard = pattern.Contains('*'),
                IsEnabled = true
            });
        }
    }
}
