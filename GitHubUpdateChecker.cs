using System.Net.Http;
using System.Reflection;

namespace GenshinImpactAutoMusic;

public sealed class GitHubUpdateChecker
{
    private const string ReleasesUrl = "https://api.github.com/repos/WilliamW1979/GenshinImpactAutoMusic/releases/latest";
    private readonly HttpClient Client = new();

    public GitHubUpdateChecker() => Client.DefaultRequestHeaders.UserAgent.ParseAdd("GenshinImpactAutoMusic");

    public async Task<(bool UpdateAvailable, string LatestVersion, string ReleaseUrl)> CheckAsync()
    {
        string body = await Client.GetStringAsync(ReleasesUrl);
        string latestTag = ExtractField(body, "tag_name");
        string releaseUrl = ExtractField(body, "html_url");
        if (string.IsNullOrEmpty(latestTag)) return (false, "", "");
        Version current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
        Version latest = Version.TryParse(latestTag.TrimStart('v', 'V'), out Version? parsed) ? parsed : current;
        return (latest > current, latestTag, releaseUrl);
    }

    private static string ExtractField(string body, string field)
    {
        int keyIndex = body.IndexOf($"\"{field}\"", StringComparison.Ordinal);
        if (keyIndex < 0) return "";
        int valueStart = body.IndexOf('"', body.IndexOf(':', keyIndex) + 1) + 1;
        int valueEnd = body.IndexOf('"', valueStart);
        return valueStart <= 0 || valueEnd < 0 ? "" : body[valueStart..valueEnd];
    }
}
