using SmartRouter.Core.Models;

namespace SmartRouter.Core.Latency;

public static class ProviderRegistry
{
    public static List<DnsProvider> GetDefaultProviders() =>
    [
        new DnsProvider
        {
            Id = "shecan",
            NameEn = "Shecan",
            NameFa = "شکن (Shecan)",
            PrimaryIp = "178.22.122.100",
            SecondaryIp = "185.51.200.2",
            IsAntiSanction = true,
            IsGamingSpecialized = false,
            DescriptionEn = "Popular Iranian anti-sanction DNS for developers & services",
            DescriptionFa = "سرویس تحریم‌شکن پرکاربرد برای برنامه‌نویسان و پلتفرم‌های تحریمی"
        },
        new DnsProvider
        {
            Id = "electro",
            NameEn = "Electro",
            NameFa = "الکترو (Electro)",
            PrimaryIp = "78.157.42.101",
            SecondaryIp = "78.157.42.100",
            IsAntiSanction = true,
            IsGamingSpecialized = true,
            DescriptionEn = "Anti-sanction & low latency DNS for gamers & developers",
            DescriptionFa = "سرویس رفع تحریم الکترو با پینگ بهینه برای بازی و توسعه"
        },
        new DnsProvider
        {
            Id = "radar",
            NameEn = "Radar Game",
            NameFa = "رادار گیم (Radar)",
            PrimaryIp = "10.202.10.10",
            SecondaryIp = "10.202.10.11",
            IsAntiSanction = true,
            IsGamingSpecialized = true,
            DescriptionEn = "Specialized gaming anti-sanction and ping optimization DNS",
            DescriptionFa = "دی‌ان‌اس تخصصی کاهش پینگ و رفع تحریم بازی‌های آنلاین"
        },
        new DnsProvider
        {
            Id = "403online",
            NameEn = "403.online",
            NameFa = "سرویس ۴۰۳ (403.online)",
            PrimaryIp = "10.202.10.202",
            SecondaryIp = "10.202.10.102",
            IsAntiSanction = true,
            IsGamingSpecialized = false,
            DescriptionEn = "Dedicated anti-sanction platform for developer toolchains",
            DescriptionFa = "سرویس رفع خطای ۴۰۳ اختصاصی برنامه‌نویسان و ابزارهای توسعه"
        },
        new DnsProvider
        {
            Id = "begzar",
            NameEn = "Begzar",
            NameFa = "بگذر (Begzar)",
            PrimaryIp = "185.55.226.26",
            SecondaryIp = "185.55.225.25",
            IsAntiSanction = true,
            IsGamingSpecialized = false,
            DescriptionEn = "Software developer anti-sanction resolution service",
            DescriptionFa = "سامانه بگذر برای رفع تحریم محیط‌های نرم‌افزاری"
        },
        new DnsProvider
        {
            Id = "cloudflare",
            NameEn = "Cloudflare (1.1.1.1)",
            NameFa = "کلودفلر (Cloudflare)",
            PrimaryIp = "1.1.1.1",
            SecondaryIp = "1.0.0.1",
            DohUrl = "https://cloudflare-dns.com/dns-query",
            IsAntiSanction = false,
            IsGamingSpecialized = false,
            DescriptionEn = "Ultra-fast global Anycast DNS with privacy protection",
            DescriptionFa = "سریع‌ترین دی‌ان‌اس جهانی امن و بدون ردگیری"
        },
        new DnsProvider
        {
            Id = "google",
            NameEn = "Google Public DNS",
            NameFa = "دی‌ان‌اس گوگل (Google)",
            PrimaryIp = "8.8.8.8",
            SecondaryIp = "8.8.4.4",
            DohUrl = "https://dns.google/dns-query",
            IsAntiSanction = false,
            IsGamingSpecialized = false,
            DescriptionEn = "Globally distributed, resilient Google Public DNS",
            DescriptionFa = "شبکه جهانی با پایداری حداکثری و پهنای باند بالا"
        },
        new DnsProvider
        {
            Id = "quad9",
            NameEn = "Quad9 (9.9.9.9)",
            NameFa = "کواد۹ (Quad9)",
            PrimaryIp = "9.9.9.9",
            SecondaryIp = "149.112.112.112",
            DohUrl = "https://dns.quad9.net/dns-query",
            IsAntiSanction = false,
            IsGamingSpecialized = false,
            DescriptionEn = "Security-oriented global DNS with threat blocking",
            DescriptionFa = "دی‌ان‌اس امنیتی جهانی همراه با مقابله با بدافزارها"
        },
        new DnsProvider
        {
            Id = "opendns",
            NameEn = "Cisco OpenDNS",
            NameFa = "سیسکو اوپن‌دی‌ان‌اس (OpenDNS)",
            PrimaryIp = "208.67.222.222",
            SecondaryIp = "208.67.220.220",
            IsAntiSanction = false,
            IsGamingSpecialized = false,
            DescriptionEn = "Reliable Cisco Anycast enterprise DNS infrastructure",
            DescriptionFa = "زیرساخت نام‌گذاری اینترنت شرکت سیسکو با قابلیت اطمینان بالا"
        }
    ];

    public static List<DnsProvider> GetAllProviders(AppConfig config)
    {
        var list = GetDefaultProviders();
        if (config.CustomProviders != null && config.CustomProviders.Count > 0)
        {
            list.AddRange(config.CustomProviders);
        }
        return list;
    }

    public static DnsProvider? FindById(string id, AppConfig config)
    {
        return GetAllProviders(config).FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
