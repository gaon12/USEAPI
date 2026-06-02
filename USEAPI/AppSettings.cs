using System.Text.Json;

namespace USEAPI;

public sealed class AppSettings
{
    public const string DefaultHomeUrl = "https://www.naver.com/";

    public string HomeUrl { get; set; } = DefaultHomeUrl;
}

public sealed class AppSettingsStore
{
    private const string LegacyUrlPath = @"textFile\URL.txt";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string settingsPath;
    private readonly string legacyBasePath;

    public AppSettingsStore()
        : this(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "USEAPI", "settings.json"),
            AppContext.BaseDirectory)
    {
    }

    public AppSettingsStore(string settingsPath, string legacyBasePath)
    {
        this.settingsPath = settingsPath ?? throw new ArgumentNullException(nameof(settingsPath));
        this.legacyBasePath = legacyBasePath ?? throw new ArgumentNullException(nameof(legacyBasePath));
    }

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                settings.HomeUrl = NormalizeHomeUrl(settings.HomeUrl);
                return settings;
            }

            var migratedSettings = new AppSettings
            {
                HomeUrl = NormalizeHomeUrl(ReadLegacyHomeUrl())
            };
            Save(migratedSettings);
            return migratedSettings;
        }
        catch (Exception ex)
        {
            ErrorLog.Write(ex);
            return new AppSettings();
        }
    }

    public bool Save(AppSettings settings, out string message)
    {
        try
        {
            Save(settings);
            message = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            ErrorLog.Write(ex);
            message = ex.Message;
            return false;
        }
    }

    private void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.HomeUrl = NormalizeHomeUrl(settings.HomeUrl);
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    private string? ReadLegacyHomeUrl()
    {
        var legacyPath = Path.Combine(legacyBasePath, LegacyUrlPath);
        return File.Exists(legacyPath)
            ? File.ReadLines(legacyPath).FirstOrDefault()
            : AppSettings.DefaultHomeUrl;
    }

    private static string NormalizeHomeUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ? uri.AbsoluteUri
                : AppSettings.DefaultHomeUrl;
    }
}
