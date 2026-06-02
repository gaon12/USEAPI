using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace USEAPI
{
    public sealed class AppSettings
    {
        public const string DefaultHomeUrl = "https://www.naver.com/";

        public AppSettings()
        {
            HomeUrl = DefaultHomeUrl;
        }

        public string HomeUrl { get; set; }
    }

    public sealed class AppSettingsStore
    {
        private const string LegacyUrlPath = @"textFile\URL.txt";

        private readonly string settingsPath;
        private readonly string legacyBasePath;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public AppSettingsStore()
            : this(
                  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "USEAPI", "settings.json"),
                  AppDomain.CurrentDomain.BaseDirectory)
        {
        }

        public AppSettingsStore(string settingsPath, string legacyBasePath)
        {
            if (settingsPath == null)
            {
                throw new ArgumentNullException("settingsPath");
            }

            if (legacyBasePath == null)
            {
                throw new ArgumentNullException("legacyBasePath");
            }

            this.settingsPath = settingsPath;
            this.legacyBasePath = legacyBasePath;
        }

        public AppSettings Load()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath, Encoding.UTF8);
                    var settings = serializer.Deserialize<AppSettings>(json);
                    if (settings == null)
                    {
                        settings = new AppSettings();
                    }
                    settings.HomeUrl = NormalizeHomeUrl(settings.HomeUrl);
                    return settings;
                }

                var migratedUrl = ReadLegacyHomeUrl();
                var migratedSettings = new AppSettings { HomeUrl = NormalizeHomeUrl(migratedUrl) };
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
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            settings.HomeUrl = NormalizeHomeUrl(settings.HomeUrl);
            var directory = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(settingsPath, serializer.Serialize(settings), Encoding.UTF8);
        }

        private string ReadLegacyHomeUrl()
        {
            var legacyPath = Path.Combine(legacyBasePath, LegacyUrlPath);
            if (!File.Exists(legacyPath))
            {
                return AppSettings.DefaultHomeUrl;
            }

            string[] lines = File.ReadAllLines(legacyPath, Encoding.UTF8);
            return lines.Length > 0 ? lines[0] : null;
        }

        private static string NormalizeHomeUrl(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return uri.AbsoluteUri;
            }

            return AppSettings.DefaultHomeUrl;
        }
    }
}
