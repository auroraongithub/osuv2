using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.AuthlibInjection.Configuration;

public class AuthlibRulesetConfigManager(SettingsStore store, RulesetInfo ruleset, int? variant = null)
    : RulesetConfigManager<AuthlibRulesetSettings>(store,
        ruleset, variant)
{
    protected override void InitialiseDefaults()
    {
        base.InitialiseDefaults();

        SetDefault(AuthlibRulesetSettings.ApiUrl, "https://lazer.salamithecat.com");
        SetDefault(AuthlibRulesetSettings.WebsiteUrl, "https://salamithecat.com");
        SetDefault(AuthlibRulesetSettings.ClientId, "5");
        SetDefault(AuthlibRulesetSettings.ClientSecret, "1f280646db1066b26f3179497c64db67");
        SetDefault(AuthlibRulesetSettings.SpectatorUrl, string.Empty);
        SetDefault(AuthlibRulesetSettings.MultiplayerUrl, string.Empty);
        SetDefault(AuthlibRulesetSettings.MetadataUrl, string.Empty);
        SetDefault(AuthlibRulesetSettings.BeatmapSubmissionServiceUrl, string.Empty);
        SetDefault(AuthlibRulesetSettings.DisableSentryLogger, true);
        SetDefault(AuthlibRulesetSettings.NonG0V0Server, false);
    }
}

public enum AuthlibRulesetSettings
{
    ApiUrl,
    WebsiteUrl,
    ClientId,
    ClientSecret,
    SpectatorUrl,
    MultiplayerUrl,
    MetadataUrl,
    BeatmapSubmissionServiceUrl,
    DisableSentryLogger,
    NonG0V0Server,
}
