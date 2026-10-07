using SmartRouter.Core.Localization;
using Xunit;

namespace SmartRouter.Tests;

public class LocalizationTests
{
    [Fact]
    public void English_And_Persian_Keys_AreAvailable()
    {
        var appTitleEn = Strings.Get("AppTitle", "en");
        var appTitleFa = Strings.Get("AppTitle", "fa");

        Assert.Equal("Smart Anti-Sanction & Latency Router", appTitleEn);
        Assert.Equal("جعبه‌ابزار هوشمند رفع تحریم و بهینه‌سازی مسیر شبکه", appTitleFa);
    }

    [Fact]
    public void LocalizationService_LanguageSwitch_UpdatesValue()
    {
        var service = LocalizationService.Instance;
        service.CurrentLanguage = "en";
        Assert.Equal("Start Router", service["Start"]);

        service.CurrentLanguage = "fa";
        Assert.Equal("شروع مسیریاب", service["Start"]);
    }
}
