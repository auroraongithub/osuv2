using osu.Game.Rulesets.AuthlibInjection.Extensions;

namespace osu.Game.Rulesets.AuthlibInjection.Configuration;

public class AuthlibRulesetConfig
{
    public const string CONFIG_FILE_NAME = "authlib_local_config.json";
    public AuthlibRulesetConfig() { }

    public AuthlibRulesetConfig(string apiUrl,
                                string websiteUrl,
                                string clientId,
                                string clientSecret,
                                string spectatorUrl,
                                string multiplayerUrl,
                                string metadataUrl,
                                string beatmapSubmissionServiceUrl,
                                bool disableSentryLogger, bool nonG0V0Server)
    {
        ApiUrl = apiUrl.RemoveSuffix("/");
        WebsiteUrl = websiteUrl.RemoveSuffix("/");
        ClientId = clientId;
        ClientSecret = clientSecret;
        SpectatorUrl = spectatorUrl.RemoveSuffix("/");
        MultiplayerUrl = multiplayerUrl.RemoveSuffix("/");
        MetadataUrl = metadataUrl.RemoveSuffix("/");
        BeatmapSubmissionServiceUrl = beatmapSubmissionServiceUrl.RemoveSuffix("/");
        DisableSentryLogger = disableSentryLogger;
        NonG0V0Server = nonG0V0Server;
    }

    public string ApiUrl { get; set; } = "https://lazer.salamithecat.com";
    public string WebsiteUrl { get; set; } = "https://salamithecat.com";
    public string ClientId { get; set; } = "5";
    public string ClientSecret { get; set; } = "1f280646db1066b26f3179497c64db67";
    public string SpectatorUrl { get; set; } = string.Empty;
    public string MultiplayerUrl { get; set; } = string.Empty;
    public string MetadataUrl { get; set; } = string.Empty;
    public string BeatmapSubmissionServiceUrl { get; set; } = string.Empty;
    public bool DisableSentryLogger { get; set; } = true;
    public bool NonG0V0Server { get; set; } = false;
}
