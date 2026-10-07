using System.Diagnostics;
using System.Net;
using SmartRouter.Core.Models;

namespace SmartRouter.Core.Diagnostics;

public class SanctionDiagnosticRunner
{
    private static readonly HttpClient HttpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = DecompressionMethods.All
    })
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public static List<DiagnosticTarget> GetDefaultTargets() =>
    [
        new DiagnosticTarget
        {
            Id = "docker",
            Name = "Docker Hub Registry",
            Url = "https://registry-1.docker.io/v2/",
            Category = RuleCategory.Developer,
            ExpectedStatusHint = "200 or 401 (Auth required, unblocked)"
        },
        new DiagnosticTarget
        {
            Id = "openai",
            Name = "OpenAI API",
            Url = "https://api.openai.com/v1/models",
            Category = RuleCategory.AiCloud,
            ExpectedStatusHint = "401 (Unblocked API endpoint)"
        },
        new DiagnosticTarget
        {
            Id = "android",
            Name = "Android Developers",
            Url = "https://developer.android.com",
            Category = RuleCategory.Developer,
            ExpectedStatusHint = "200 OK"
        },
        new DiagnosticTarget
        {
            Id = "claude",
            Name = "Anthropic Claude",
            Url = "https://api.anthropic.com/v1/messages",
            Category = RuleCategory.AiCloud,
            ExpectedStatusHint = "401 (Unblocked API endpoint)"
        },
        new DiagnosticTarget
        {
            Id = "unity",
            Name = "Unity Engine License API",
            Url = "https://license.unity3d.com",
            Category = RuleCategory.Developer,
            ExpectedStatusHint = "200 or 404"
        },
        new DiagnosticTarget
        {
            Id = "steam",
            Name = "Steam Community",
            Url = "https://steamcommunity.com",
            Category = RuleCategory.Gaming,
            ExpectedStatusHint = "200 or 302"
        }
    ];

    public async Task<DiagnosticResult> TestTargetAsync(DiagnosticTarget target, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, target.Url);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) SmartRouter/1.0");

            using var resp = await HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            sw.Stop();

            var statusCode = (int)resp.StatusCode;
            var latency = Math.Round(sw.Elapsed.TotalMilliseconds, 1);

            // 403 Forbidden or 451 Unavailable For Legal Reasons usually signifies geo-block
            if (statusCode == 403 || statusCode == 451)
            {
                return new DiagnosticResult
                {
                    TargetId = target.Id,
                    TargetName = target.Name,
                    Url = target.Url,
                    IsAccessible = false,
                    StatusCode = statusCode,
                    LatencyMs = latency,
                    MessageEn = $"Geo-Blocked / 403 Forbidden ({latency} ms)",
                    MessageFa = $"تحریم شده / خطای ۴۰۳ یا انسداد منطقه‌ای ({latency} میلی‌ثانیه)"
                };
            }

            // Status 200, 301, 302, 401, or 405 (API key missing or method not allowed means host was successfully reached!)
            var isAccessible = statusCode is >= 200 and < 400 || statusCode is 401 or 405;

            return new DiagnosticResult
            {
                TargetId = target.Id,
                TargetName = target.Name,
                Url = target.Url,
                IsAccessible = isAccessible,
                StatusCode = statusCode,
                LatencyMs = latency,
                MessageEn = isAccessible ? $"Accessible & Unblocked ({statusCode}, {latency} ms)" : $"Http Status {statusCode} ({latency} ms)",
                MessageFa = isAccessible ? $"آزاد و رفع‌تحریم‌شده ({statusCode}، {latency} میلی‌ثانیه)" : $"کد پاسخ {statusCode} ({latency} میلی‌ثانیه)"
            };
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            return new DiagnosticResult
            {
                TargetId = target.Id,
                TargetName = target.Name,
                Url = target.Url,
                IsAccessible = false,
                StatusCode = 0,
                LatencyMs = -1,
                MessageEn = "Connection Timeout",
                MessageFa = "پایان مهلت زمانی ارتباط (Timeout)"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new DiagnosticResult
            {
                TargetId = target.Id,
                TargetName = target.Name,
                Url = target.Url,
                IsAccessible = false,
                StatusCode = 0,
                LatencyMs = -1,
                MessageEn = $"Error: {ex.Message}",
                MessageFa = $"خطای اتصال: {ex.Message}"
            };
        }
    }

    public async Task<List<DiagnosticResult>> RunAllAsync(Action<DiagnosticResult>? onProgress = null, CancellationToken ct = default)
    {
        var targets = GetDefaultTargets();
        var results = new List<DiagnosticResult>();

        foreach (var t in targets)
        {
            if (ct.IsCancellationRequested) break;
            var res = await TestTargetAsync(t, ct);
            results.Add(res);
            onProgress?.Invoke(res);
        }

        return results;
    }
}
