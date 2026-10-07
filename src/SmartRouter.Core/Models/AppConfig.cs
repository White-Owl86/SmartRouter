using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartRouter.Core.Models;

public class AppConfig
{
    public string ListenAddress { get; set; } = "127.0.0.1";
    public int DnsPort { get; set; } = 53;
    public int HttpProxyPort { get; set; } = 8085;
    public int SocksProxyPort { get; set; } = 1085;

    public bool EnableDnsProxy { get; set; } = true;
    public bool EnableHttpProxy { get; set; } = false;
    public bool EnableSocksProxy { get; set; } = false;

    public RoutingMode Mode { get; set; } = RoutingMode.SmartSplitDns;
    public string SelectedAntiSanctionProviderId { get; set; } = "shecan";
    public string SelectedCleanDnsProviderId { get; set; } = "cloudflare";

    public string Language { get; set; } = "fa"; // "fa" or "en"
    public bool AutoApplySystemDns { get; set; } = true;
    public bool AutoSelectLowestLatency { get; set; } = false;

    public bool EnableDnsCache { get; set; } = true;
    public int DnsCacheTtlSeconds { get; set; } = 300;

    public bool DeveloperCategoryEnabled { get; set; } = true;
    public bool AiCloudCategoryEnabled { get; set; } = true;
    public bool GamingCategoryEnabled { get; set; } = true;
    public bool BypassLocalTraffic { get; set; } = true;

    public List<DomainRule> CustomRules { get; set; } = [];
    public List<DnsProvider> CustomProviders { get; set; } = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static AppConfig Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
        }
        catch
        {
            // Fall back to default on parse/read error
        }
        return new AppConfig();
    }

    public void Save(string path)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch
        {
            // Handle logging or graceful failure in callers
        }
    }
}
