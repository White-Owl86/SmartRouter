# جعبه‌ابزار هوشمند رفع تحریم و بهینه‌سازی مسیر شبکه
## Smart Anti-Sanction & Latency Router (Cross-Platform)

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/UI-Avalonia_12-blue.svg)](https://avaloniaui.net/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-green.svg)]()
[![Bilingual](https://img.shields.io/badge/Language-English%20%7C%20فارسی-orange.svg)]()
[![Tests](https://img.shields.io/badge/Tests-17%20Passed-brightgreen.svg)]()

---

### [FA] فارسی

**جعبه‌ابزار هوشمند رفع تحریم و بهینه‌سازی مسیر شبکه (Smart Anti-Sanction & Latency Router)** یک ابزار کراس‌پلتفرم قدرتمند، سبک و دو‌زبانه (فارسی و انگلیسی) است که ترافیک سرویس‌های تحریم‌شده توسعه‌دهندگان و گیمرها (مانند داکر هاب، هوش مصنوعی OpenAI، اندروید دولوپرز، کلود آنتروپیک، استیم، یونیتی و...) را به شکل هوشمند از ترافیک عادی و داخلی تفکیک کرده و از طریق دی‌ان‌اس ضدتحریم کم‌پینگ عبور می‌دهد.

#### ویژگی‌های کلیدی:
1. **تفکیک هوشمند ترافیک (Smart Split-Routing)**:
   - درخواست‌های تحریمی (Docker, OpenAI, Android, Claude, Unity, Steam, AWS, ...) از دی‌ان‌اس‌های بهینه ضدتحریم عبور می‌کنند.
   - درخواست‌های آزاد و عادی از سریع‌ترین دی‌ان‌اس جهانی (Cloudflare 1.1.1.1, Google 8.8.8.8, DoH) عبور می‌کنند.
   - درخواست‌های محلی و دامنه‌های داخلی (`.ir` و شبکه‌های محلی LAN) بدون هیچ‌گونه سربار مستقیماً باز می‌شوند (Bypass).
2. **سنجش پینگ و بنچمارک زنده ارائه‌دهندگان (Latency Benchmark)**:
   - سنجش بلادرنگ پینگ به شکن (Shecan)، الکترو (Electro)، رادار گیم (Radar Game)، سرویس ۴۰۳ (403.online)، بگذر (Begzar)، کلودفلر، گوگل و کواد۹.
   - قابلیت انتخاب خودکار سریع‌ترین ارائه‌دهنده با کمترین پینگ.
3. **آزمایشگاه عیب‌یابی تحریم (Sanction Diagnostics Lab)**:
   - تست زنده سلامت دور زدن تحریم با برقراری ارتباط واقعی با Docker Registry, OpenAI API, Android Developers, Claude API, Unity, Steam.
4. **رابط کاربری دوزبانه و مدرن (Bilingual Modern UI)**:
   - رابط گرافیکی دسکتاپ بر پایه **Avalonia UI** با پشتیبانی کامل از چینش راست‌به‌چپ (RTL) برای زبان فارسی و چپ‌به‌راست (LTR) برای انگلیسی.
   - تغییر زبان لحظه‌ای با یک کلیک.
5. **موتور خط فرمان کامل (Headless CLI Daemon)**:
   - قابلیت اجرا در سرورهای لینوکس، کانتینرها، WSL و محیط‌های متنی.
6. **تنظیم خودکار کارت شبکه و بازگردانی امن (Fail-Safe Rollback)**:
   - پشتیبان‌گیری از تنظیمات دی‌ان‌اس کارت شبکه سیستم‌عامل و بازگردانی خودکار در زمان خروج یا بسته شدن برنامه.

---

### [EN] English

**Smart Anti-Sanction & Latency Router** is an intelligent, high-performance, cross-platform network router designed to bypass international geo-blocking and optimize latency for developers, gamers, and businesses.

#### Key Features:
- **Intelligent Split-DNS**: Routes only sanctioned domains (Docker, OpenAI, Android Developers, Claude, Unity, Steam, etc.) through anti-sanction providers while routing ordinary traffic through ultra-fast global DNS (Cloudflare, Google, DoH).
- **Zero Overhead on Local Traffic**: Direct bypass for local intranet and national domains.
- **Latency Benchmark & Auto-Selection**: Live pings and ranks all anti-sanction and Anycast DNS providers.
- **Sanction Diagnostics Suite**: Handshake verification testing Docker Hub, OpenAI, Android Developers, Claude, Unity, and Steam.
- **Fully Bilingual (English & Persian)**: Native RTL layout support for Persian and LTR for English with instant runtime toggle.
- **Cross-Platform**: Windows 10/11 and Linux (desktop GUI + headless server CLI).
- **Fail-Safe DNS Manager**: Backs up OS network configuration and restores it gracefully on shutdown.

---

### معماری پروژه / Solution Architecture

```
F:\Smart Anti-Sanction & Latency Router\
├── SmartRouter.slnx                         # Solution file (.NET 10)
├── run-gui.bat / run-gui.sh                # Script to run Avalonia UI
├── run-cli.bat / run-cli.sh                # Script to run CLI daemon
├── src/
│   ├── SmartRouter.Core/                   # Core business logic library
│   │   ├── Models/                         # Data contracts (AppConfig, DnsProvider, DomainRule...)
│   │   ├── Localization/                   # Bilingual dictionary (EN / FA) & service
│   │   ├── Rules/                          # Split-rules engine, categories, pattern matcher
│   │   ├── Dns/                            # RFC 1035 packet parser, high-speed cache, forwarders, UDP server
│   │   ├── Latency/                        # Provider registry & latency benchmark engine
│   │   ├── Network/                        # Cross-platform Windows & Linux DNS adapter managers
│   │   ├── Diagnostics/                    # Sanction reachability test suite
│   │   └── Services/                       # Central RouterService coordinator
│   │
│   ├── SmartRouter.UI/                     # Cross-platform Avalonia Desktop application
│   │   ├── ViewModels/                     # MVVM ViewModel with reactive properties
│   │   ├── MainWindow.axaml                # Responsive modern dark UI
│   │   └── MainWindow.axaml.cs             # UI interactions
│   │
│   └── SmartRouter.Cli/                    # Cross-platform command line daemon
│       └── Program.cs                      # CLI runner, ping, diag, rules, daemon
│
└── tests/
    └── SmartRouter.Tests/                  # Comprehensive unit tests (xUnit)
        ├── DnsPacketTests.cs               # Packet parsing & ID rewriting tests
        ├── DomainRuleEngineTests.cs        # Matcher & category tests
        ├── LocalizationTests.cs            # Bilingual string tests
        └── DnsCacheTests.cs                # Cache hit & TTL tests
```

---

### نحوه اجرا / How to Run

#### ۱. اجرای نسخه گرافیکی (Desktop GUI):
- **ویندوز**: دوبار کلیک روی فایل `run-gui.bat` یا دستور:
  ```powershell
  dotnet run --project src/SmartRouter.UI/SmartRouter.UI.csproj -c Release
  ```
- **لینوکس**:
  ```bash
  chmod +x run-gui.sh
  ./run-gui.sh
  ```

#### ۲. اجرای نسخه ترمینال / خط فرمان (CLI Daemon):
- **اجرای مسیریاب (پیش‌فرض)**:
  ```powershell
  dotnet run --project src/SmartRouter.Cli/SmartRouter.Cli.csproj -- run
  ```
- **تست پینگ و بنچمارک ارائه‌دهندگان**:
  ```powershell
  dotnet run --project src/SmartRouter.Cli/SmartRouter.Cli.csproj -- ping
  ```
- **اجرای آزمایشگاه عیب‌یابی تحریم**:
  ```powershell
  dotnet run --project src/SmartRouter.Cli/SmartRouter.Cli.csproj -- test
  ```
- **مشاهده قوانین فعال**:
  ```powershell
  dotnet run --project src/SmartRouter.Cli/SmartRouter.Cli.csproj -- rules
  ```
- **تغییر زبان به انگلیسی یا فارسی در خط فرمان**:
  ```powershell
  dotnet run --project src/SmartRouter.Cli/SmartRouter.Cli.csproj -- ping --lang en
  ```

#### ۳. اجرای تست‌های خودکار (Unit Tests):
```powershell
dotnet test
```

---

### ارائه‌دهندگان پیش‌فرض / Included Providers

| ارائه‌دهنده / Provider | آی‌پی اصلی / Primary IP | نوع / Type | توضیحات / Description |
|---|---|---|---|
| **بگذر (Begzar)** | `185.55.226.26` | Anti-Sanction | رفع تحریم محیط‌های نرم‌افزاری |
| **شکن (Shecan)** | `178.22.122.100` | Anti-Sanction | رفع تحریم پرکاربرد توسعه‌دهندگان |
| **سرویس ۴۰۳ (403.online)** | `10.202.10.202` | Anti-Sanction | حل خطای ۴۰۳ ابزارهای مهندسی |
| **الکترو (Electro)** | `78.157.42.101` | Gaming / Anti-Sanction | پینگ بهینه برای بازی و توسعه |
| **رادار گیم (Radar Game)** | `10.202.10.10` | Gaming Low-Ping | کاهش پینگ و رفع تحریم بازی |
| **Cloudflare** | `1.1.1.1` | Global Clean (DoH) | سریع‌ترین دی‌ان‌اس جهانی Anycast |
| **Google Public DNS** | `8.8.8.8` | Global Clean (DoH) | شبکه جهانی با پایداری حداکثری |
| **Quad9** | `9.9.9.9` | Global Clean (DoH) | دی‌ان‌اس امنیتی با مسدودسازی بدافزار |
| **Cisco OpenDNS** | `208.67.222.222` | Global Clean | زیرساخت نام‌گذاری معتبر سیسکو |

---

### مجوز / License
MIT License - کاملاً رایگان و متن‌باز برای تمام توسعه‌دهندگان و کاربران.
