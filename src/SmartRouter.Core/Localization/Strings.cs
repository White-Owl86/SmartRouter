namespace SmartRouter.Core.Localization;

public static class Strings
{
    private static readonly Dictionary<string, string> En = new(StringComparer.OrdinalIgnoreCase)
    {
        // General
        ["AppTitle"] = "Smart Anti-Sanction & Latency Router",
        ["AppSubtitle"] = "Intelligent Network Split-Router for Developers, Gamers & Businesses",
        ["Status"] = "Status",
        ["StatusActive"] = "Active & Routing",
        ["StatusStopped"] = "Stopped",
        ["StatusStarting"] = "Starting Engine...",
        ["StatusStopping"] = "Stopping Engine...",
        ["StatusError"] = "Error Encountered",
        ["Start"] = "Start Router",
        ["Stop"] = "Stop Router",
        ["Restart"] = "Restart",
        ["Close"] = "Close",
        ["Cancel"] = "Cancel",
        ["Save"] = "Save Settings",
        ["Reset"] = "Reset to Defaults",
        ["Refresh"] = "Refresh",
        ["Apply"] = "Apply",

        // Navigation Tabs
        ["TabDashboard"] = "Dashboard",
        ["TabProviders"] = "DNS & Latency",
        ["TabRules"] = "Rules & Services",
        ["TabDiagnostics"] = "Sanction Test",
        ["TabLogs"] = "Live Activity",
        ["TabSettings"] = "Settings",

        // Dashboard
        ["TotalQueries"] = "Total Queries",
        ["SanctionedRouted"] = "Sanctions Bypassed",
        ["CleanQueries"] = "Direct / Clean DNS",
        ["CacheHitRate"] = "Cache Hits",
        ["ActiveProvider"] = "Active Anti-Sanction DNS",
        ["CleanProvider"] = "Clean Global DNS",
        ["CurrentPing"] = "Provider Ping",
        ["ModeLabel"] = "Routing Mode",
        ["QuickTestHeader"] = "Real-Time Service Reachability",
        ["QuickTestDocker"] = "Docker Hub",
        ["QuickTestOpenAI"] = "OpenAI API",
        ["QuickTestAndroid"] = "Android Developers",
        ["QuickTestClaude"] = "Anthropic Claude",
        ["QuickTestUnity"] = "Unity Engine",
        ["QuickTestSteam"] = "Steam Community",

        // Modes
        ["ModeSmartSplit"] = "Smart Split (Anti-Sanction for Blocked, Fast Global for Rest)",
        ["ModeFullAntiSanction"] = "Full Route (All DNS through Anti-Sanction Provider)",
        ["ModeFastestGlobal"] = "Global Turbo (All DNS through Lowest-Latency Global DNS)",
        ["ModeCustom"] = "Custom Upstream DNS",

        // Providers
        ["ProvidersTitle"] = "DNS Upstream Benchmark & Selection",
        ["TestAllPings"] = "Test All Latencies",
        ["AutoSelectFastest"] = "Auto-Select Lowest Latency Provider",
        ["ProviderName"] = "Provider",
        ["ProviderType"] = "Type",
        ["ProviderLatency"] = "Latency",
        ["ProviderPrimary"] = "Primary IP",
        ["ProviderSecondary"] = "Secondary IP",
        ["TypeAntiSanction"] = "Anti-Sanction",
        ["TypeGlobal"] = "Global / Clean",
        ["TypeGaming"] = "Gaming Low-Ping",
        ["TestingPing"] = "Pinging...",
        ["Unreachable"] = "Unreachable / Timeout",

        // Rules
        ["RulesTitle"] = "Smart Domain Split Rules",
        ["CategoryDeveloper"] = "Developer & Repositories (Docker, Gradle, Oracle, JetBrains, NPM...)",
        ["CategoryAiCloud"] = "AI & Cloud Platforms (OpenAI, Claude, AWS, Firebase, Supabase...)",
        ["CategoryGaming"] = "Gaming & Entertainment (Steam, Epic, Riot, Blizzard, Discord...)",
        ["AddCustomDomain"] = "Add Custom Domain",
        ["EnterDomainHint"] = "Enter domain or wildcard (e.g., *.docker.io, openai.com)",
        ["DeleteRule"] = "Delete",
        ["SearchRules"] = "Search rules...",
        ["TotalRulesCount"] = "Active Patterns",

        // Diagnostics
        ["DiagnosticsTitle"] = "Sanction & Geo-Block Diagnostic Suite",
        ["DiagnosticsDesc"] = "Tests actual HTTP/HTTPS handshakes with heavily sanctioned targets to confirm bypass effectiveness.",
        ["RunDiagnostic"] = "Run All Checks",
        ["DiagnosticStatus"] = "Result",
        ["DiagnosticPassed"] = "Bypassed & Accessible",
        ["DiagnosticBlocked"] = "Sanctioned / Blocked (403/Forbidden)",
        ["DiagnosticFailed"] = "Connection Failed / Timeout",

        // Logs
        ["LogsTitle"] = "Live Intercepted Queries",
        ["ClearLogs"] = "Clear Logs",
        ["ExportLogs"] = "Export CSV",
        ["LogColTime"] = "Time",
        ["LogColDomain"] = "Domain",
        ["LogColType"] = "Record",
        ["LogColRouting"] = "Route Action",
        ["LogColUpstream"] = "Upstream Used",
        ["LogColTimeMs"] = "Time (ms)",
        ["LogColStatus"] = "Status",
        ["LogSanctioned"] = "Sanction Bypassed",
        ["LogClean"] = "Direct Clean",
        ["LogCached"] = "Cached",

        // Settings
        ["SettingsTitle"] = "Application Settings",
        ["LanguageLabel"] = "Interface Language",
        ["ListenAddressLabel"] = "Listen IP Address",
        ["DnsPortLabel"] = "DNS Server Port (Standard: 53)",
        ["AutoApplyDnsLabel"] = "Automatically configure System Network DNS",
        ["BypassIntranetLabel"] = "Direct bypass for local intranet / .ir domains",
        ["EnableCacheLabel"] = "Enable in-memory high-speed DNS cache",
        ["TtlLabel"] = "Cache TTL (Seconds)",
        ["DnsPortNotice"] = "Note: Port 53 requires Administrator rights on Windows and Root/CAP_NET_BIND_SERVICE on Linux.",

        // Errors & Notifications
        ["ErrorTitle"] = "Error",
        ["SuccessTitle"] = "Success",
        ["ErrorAdminRequiredWindows"] = "Administrator privileges are required to bind to port 53 and configure Windows network adapters. Please run the program as Administrator.",
        ["ErrorRootRequiredLinux"] = "Root or sudo privileges are required to bind to port 53 and configure Linux resolv.conf. Please run with sudo.",
        ["ErrorPortInUse"] = "Port {0} is already in use by another service (e.g. Acrylic, Pi-hole, systemd-resolved, or another DNS server). Please close that service or change port.",
        ["ErrorDnsConfigFailed"] = "Failed to update network adapter DNS settings: {0}",
        ["ErrorRestoreDnsFailed"] = "Failed to restore original network DNS: {0}",
        ["ErrorInvalidDomain"] = "Please enter a valid domain name (e.g. example.com or *.example.com).",
        ["ErrorNetworkDown"] = "No active network interface with default gateway found.",
        ["DnsRestoredSuccess"] = "System DNS settings were successfully restored.",
        ["DnsAppliedSuccess"] = "Smart Router is now actively protecting and accelerating your connection.",
        ["ConfigSavedSuccess"] = "Settings saved successfully."
    };

    private static readonly Dictionary<string, string> Fa = new(StringComparer.OrdinalIgnoreCase)
    {
        // General
        ["AppTitle"] = "جعبه‌ابزار هوشمند رفع تحریم و بهینه‌سازی مسیر شبکه",
        ["AppSubtitle"] = "مسیریاب تفکیک هوشمند ترافیک برای برنامه‌نویسان، گیمرها و کسب‌وکارها",
        ["Status"] = "وضعیت",
        ["StatusActive"] = "فعال و در حال مسیریابی",
        ["StatusStopped"] = "متوقف شده",
        ["StatusStarting"] = "در حال راه‌اندازی موتور...",
        ["StatusStopping"] = "در حال توقف موتور...",
        ["StatusError"] = "خطا در عملکرد",
        ["Start"] = "شروع مسیریاب",
        ["Stop"] = "توقف مسیریاب",
        ["Restart"] = "راه‌اندازی مجدد",
        ["Close"] = "بستن",
        ["Cancel"] = "انصراف",
        ["Save"] = "ذخیره تنظیمات",
        ["Reset"] = "بازگردانی به پیش‌فرض",
        ["Refresh"] = "تازه‌سازی",
        ["Apply"] = "اعمال",

        // Navigation Tabs
        ["TabDashboard"] = "داشبورد",
        ["TabProviders"] = "دی‌ان‌اس و پینگ",
        ["TabRules"] = "قوانین و سرویس‌ها",
        ["TabDiagnostics"] = "تست رفع تحریم",
        ["TabLogs"] = "گزارش زنده",
        ["TabSettings"] = "تنظیمات",

        // Dashboard
        ["TotalQueries"] = "مجموع درخواست‌ها",
        ["SanctionedRouted"] = "سرویس‌های رفع تحریم شده",
        ["CleanQueries"] = "درخواست‌های مستقیم/تمیز",
        ["CacheHitRate"] = "پاسخ از حافظه موقت (کش)",
        ["ActiveProvider"] = "دی‌ان‌اس ضدتحریم فعال",
        ["CleanProvider"] = "دی‌ان‌اس جهانی سریع",
        ["CurrentPing"] = "پینگ ارائه‌دهنده",
        ["ModeLabel"] = "حالت مسیریابی",
        ["QuickTestHeader"] = "بررسی زنده دسترسی به سرویس‌های کلیدی",
        ["QuickTestDocker"] = "داکر هاب (Docker Hub)",
        ["QuickTestOpenAI"] = "هوش مصنوعی OpenAI",
        ["QuickTestAndroid"] = "اندروید دولوپرز (Android)",
        ["QuickTestClaude"] = "کلود آنتروپیک (Claude)",
        ["QuickTestUnity"] = "موتور یونیتی (Unity)",
        ["QuickTestSteam"] = "استیم (Steam)",

        // Modes
        ["ModeSmartSplit"] = "تفکیک هوشمند (سرویس‌های تحریمی از ضدتحریم، مابقی از دی‌ان‌اس جهانی)",
        ["ModeFullAntiSanction"] = "هدایت کامل (عبور تمام درخواست‌ها از ارائه‌دهنده ضدتحریم)",
        ["ModeFastestGlobal"] = "توربو جهانی (عبور تمام درخواست‌ها از کم‌پینگ‌ترین دی‌ان‌اس جهانی)",
        ["ModeCustom"] = "دی‌ان‌اس سفارشی کاربر",

        // Providers
        ["ProvidersTitle"] = "سنجش پینگ و انتخاب ارائه‌دهنده دی‌ان‌اس",
        ["TestAllPings"] = "تست پینگ همه ارائه‌دهندگان",
        ["AutoSelectFastest"] = "انتخاب خودکار کم‌ترین پینگ",
        ["ProviderName"] = "نام ارائه‌دهنده",
        ["ProviderType"] = "نوع",
        ["ProviderLatency"] = "پینگ (میلی‌ثانیه)",
        ["ProviderPrimary"] = "آدرس آی‌پی اول",
        ["ProviderSecondary"] = "آدرس آی‌پی دوم",
        ["TypeAntiSanction"] = "ضدتحریم (Anti-Sanction)",
        ["TypeGlobal"] = "جهانی و پاک",
        ["TypeGaming"] = "مخصوص بازی (پینگ پایین)",
        ["TestingPing"] = "در حال پینگ...",
        ["Unreachable"] = "غیرقابل دسترس / تایم‌اوت",

        // Rules
        ["RulesTitle"] = "قوانین تفکیک هوشمند دامنه‌ها",
        ["CategoryDeveloper"] = "ابزارهای برنامه‌نویسی و مخازن (Docker, Gradle, Oracle, JetBrains, NPM...)",
        ["CategoryAiCloud"] = "سکوهای ابری و هوش مصنوعی (OpenAI, Claude, AWS, Firebase, Supabase...)",
        ["CategoryGaming"] = "گیمینگ و بازی‌های ویدیویی (Steam, Epic, Riot, Blizzard, Discord...)",
        ["AddCustomDomain"] = "افزودن دامنه سفارشی",
        ["EnterDomainHint"] = "دامنه یا الگوی وایلدکارد را وارد کنید (مثال: *.docker.io یا openai.com)",
        ["DeleteRule"] = "حذف",
        ["SearchRules"] = "جستجوی قوانین...",
        ["TotalRulesCount"] = "تعداد الگوهای فعال",

        // Diagnostics
        ["DiagnosticsTitle"] = "آزمایشگاه عیب‌یابی تحریم و انسداد جغرافیایی",
        ["DiagnosticsDesc"] = "با ارسال درخواست واقعی به سرورهای سرویس‌های تحریم‌شده، سلامت دور زدن تحریم را می‌سنجد.",
        ["RunDiagnostic"] = "اجرای همه تست‌ها",
        ["DiagnosticStatus"] = "نتیجه",
        ["DiagnosticPassed"] = "تحریم شکسته شد / دسترسی آزاد",
        ["DiagnosticBlocked"] = "تحریم شده / مسدود (403 Forbidden)",
        ["DiagnosticFailed"] = "خطای ارتباط / تایم‌اوت",

        // Logs
        ["LogsTitle"] = "گزارش زنده درخواست‌های شنود و هدایت‌شده",
        ["ClearLogs"] = "پاکسازی لاگ‌ها",
        ["ExportLogs"] = "خروجی اکسل / CSV",
        ["LogColTime"] = "زمان",
        ["LogColDomain"] = "دامنه",
        ["LogColType"] = "نوع رکورد",
        ["LogColRouting"] = "عملیات مسیر",
        ["LogColUpstream"] = "سرور پاسخ‌دهنده",
        ["LogColTimeMs"] = "زمان پاسخ (ms)",
        ["LogColStatus"] = "وضعیت",
        ["LogSanctioned"] = "شکستن تحریم",
        ["LogClean"] = "اتصال مستقیم تمیز",
        ["LogCached"] = "حافظه موقت (کش)",

        // Settings
        ["SettingsTitle"] = "تنظیمات برنامه",
        ["LanguageLabel"] = "زبان رابط کاربری",
        ["ListenAddressLabel"] = "آدرس آی‌پی شنود محلی",
        ["DnsPortLabel"] = "پورت سرور دی‌ان‌اس (استاندارد: 53)",
        ["AutoApplyDnsLabel"] = "تنظیم خودکار کارت شبکه سیستم‌عامل روی این برنامه",
        ["BypassIntranetLabel"] = "عبور مستقیم ترافیک محلی و دامنه‌های داخلی (ir.) بدون تغییر",
        ["EnableCacheLabel"] = "فعال‌سازی کش فوق‌سریع در حافظه رم",
        ["TtlLabel"] = "مدت زمان ماندگاری کش (ثانیه)",
        ["DnsPortNotice"] = "توجه: گوش دادن به پورت ۵۳ در ویندوز نیازمند دسترسی Administrator و در لینوکس نیازمند مجوز Root است.",

        // Errors & Notifications
        ["ErrorTitle"] = "خطا",
        ["SuccessTitle"] = "عملیات موفق",
        ["ErrorAdminRequiredWindows"] = "برای اتصال به پورت ۵۳ و تنظیم کارت شبکه ویندوز، دسترسی Administrator الزامی است. لطفاً برنامه را با Run as Administrator اجرا کنید.",
        ["ErrorRootRequiredLinux"] = "برای اتصال به پورت ۵۳ و ویرایش فایل resolv.conf لینوکس، دسترسی Root یا Sudo لازم است.",
        ["ErrorPortInUse"] = "پورت {0} توسط برنامه دیگری (مانند systemd-resolved، Acrylic، یا سرویس DNS دیگر) در حال استفاده است. آن برنامه را ببندید یا پورت را تغییر دهید.",
        ["ErrorDnsConfigFailed"] = "خطا در اعمال تنظیمات کارت شبکه: {0}",
        ["ErrorRestoreDnsFailed"] = "خطا در بازگردانی دی‌ان‌اس پیش‌فرض سیستم‌عامل: {0}",
        ["ErrorInvalidDomain"] = "لطفاً نام دامنه معتبر وارد کنید (مثال: example.com یا *.example.com).",
        ["ErrorNetworkDown"] = "هیچ کارت شبکه فعالی با گیت‌وی پیش‌فرض یافت نشد.",
        ["DnsRestoredSuccess"] = "تنظیمات دی‌ان‌اس سیستم‌عامل با موفقیت به حالت اولیه بازگردانده شد.",
        ["DnsAppliedSuccess"] = "مسیریاب هوشمند فعال شد و ترافیک شبکه شما اکنون بهینه و ضدتحریم است.",
        ["ConfigSavedSuccess"] = "تنظیمات با موفقیت ذخیره شدند."
    };

    public static string Get(string key, string language)
    {
        var dict = language.Equals("fa", StringComparison.OrdinalIgnoreCase) ? Fa : En;
        if (dict.TryGetValue(key, out var val)) return val;
        if (En.TryGetValue(key, out var fallback)) return fallback;
        return key;
    }
}
