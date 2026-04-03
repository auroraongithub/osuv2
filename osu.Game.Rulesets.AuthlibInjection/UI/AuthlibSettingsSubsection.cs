using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.AuthlibInjection.Configuration;
using osu.Game.Rulesets.AuthlibInjection.Extensions;
using osu.Game.Rulesets.AuthlibInjection.Notifications;
using osu.Game.Rulesets.AuthlibInjection.Patches;

namespace osu.Game.Rulesets.AuthlibInjection.UI;

public partial class AuthlibSettingsSubsection(Ruleset ruleset) : RulesetSettingsSubsection(ruleset)
{
    private const int delay = 1500;
    private readonly Ruleset ruleset = ruleset;
    private AuthlibRulesetConfig authlibRulesetConfig = new();

    // Considered for distinction, batch disabling
    // ReSharper disable InconsistentNaming
    private SettingsCheckbox DisableSentryLogging = null!;
    private SettingsCheckbox NonG0V0Server = null!;

    private SettingsTextBox ApiUrl = null!;

    private SettingsTextBox BeatmapSubmissionServiceUrl = null!;

    private SettingsTextBox ClientId = null!;

    private SettingsTextBox ClientSecret = null!;

    private SettingsTextBox MetadataUrl = null!;

    private SettingsTextBox MultiplayerUrl = null!;

    private SettingsTextBox SpectatorUrl = null!;

    private SettingsTextBox WebsiteUrl = null!;
    // ReSharper restore InconsistentNaming

    private string filePath = "";

    private AuthlibRulesetConfigManager config => (AuthlibRulesetConfigManager)Config;

    protected override LocalisableString Header => ruleset.Description;

    // [CanBeNull] [Resolved] private OsuGame game { get; set; }

    [Resolved]
    protected INotificationOverlay Notifications { get; private set; } = null!;

    [BackgroundDependencyLoader]
    private void load(OsuGame game, Storage storage)
    {
        filePath = storage.GetFullPath(AuthlibRulesetConfig.CONFIG_FILE_NAME);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            authlibRulesetConfig =
                JsonConvert.DeserializeObject<AuthlibRulesetConfig>(json) ?? new AuthlibRulesetConfig();
        }

        Children =
        [
            ApiUrl = new SettingsTextBox()
            {
                LabelText = "API Url",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.ApiUrl)
            },
            WebsiteUrl = new SettingsTextBox()
            {
                LabelText = "Website Url",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.WebsiteUrl)
            },
            ClientId = new SettingsTextBox()
            {
                LabelText = "Client ID",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.ClientId)
            },
            ClientSecret = new SettingsTextBox()
            {
                LabelText = "Client Secret",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.ClientSecret)
            },
            SpectatorUrl = new SettingsTextBox()
            {
                LabelText = "Spectator Url",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.SpectatorUrl)
            },
            MultiplayerUrl = new SettingsTextBox()
            {
                LabelText = "Multiplayer Url",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.MultiplayerUrl)
            },
            MetadataUrl = new SettingsTextBox()
            {
                LabelText = "Metadata Url",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.MetadataUrl)
            },
            BeatmapSubmissionServiceUrl = new SettingsTextBox()
            {
                LabelText = "Beatmap Submission Service Url",
                Current = config.GetBindable<string>(AuthlibRulesetSettings.BeatmapSubmissionServiceUrl)
            },
            DisableSentryLogging = new SettingsCheckbox()
            {
                LabelText = "Disable Sentry Logger",
                TooltipText = "Stop sending telemetry error data to the osu! dev team.",
                Current = config.GetBindable<bool>(AuthlibRulesetSettings.DisableSentryLogger)
            },
            NonG0V0Server = new SettingsCheckbox()
            {
                LabelText = "Is non-g0v0-server",
                TooltipText = "Whether the server is a GooGuTeam/g0v0-server instance. You can view https://<api-url>/docs to identify",
                Current = config.GetBindable<bool>(AuthlibRulesetSettings.NonG0V0Server),
            },
            new SettingsButton()
            {
                Text = "Save Changes",
                Action = onSaveChanges
            },
        ];
        DisableSentryLogging.Current.BindValueChanged(e => onSentryOptOutChanged(e, game), true);
    }

    private void onSentryOptOutChanged(ValueChangedEvent<bool> e, OsuGame game)
    {
        File.WriteAllText(filePath, JsonConvert.SerializeObject(authlibRulesetConfig));

        // When switching from off to on, try to disable potentially active logger instance.
        if (e.NewValue)
        {
            DisableSentryPatch.Run(game);
        }
    }

    private void onSaveChanges()
    {
        foreach (var settingsTextBox in (SettingsTextBox[])[ApiUrl, WebsiteUrl, SpectatorUrl, MultiplayerUrl, MetadataUrl, BeatmapSubmissionServiceUrl])
        {
            if (!string.IsNullOrEmpty(settingsTextBox.Current.Value))
                settingsTextBox.Current.Value = settingsTextBox.Current.Value.RemoveSuffix("/").AddHttpsProtocol();
        }

        authlibRulesetConfig = new AuthlibRulesetConfig()
        {
            ApiUrl = ApiUrl.Current.Value,
            WebsiteUrl = WebsiteUrl.Current.Value,
            ClientId = ClientId.Current.Value,
            ClientSecret = ClientSecret.Current.Value,
            SpectatorUrl = SpectatorUrl.Current.Value,
            MultiplayerUrl = MultiplayerUrl.Current.Value,
            MetadataUrl = MetadataUrl.Current.Value,
            BeatmapSubmissionServiceUrl = BeatmapSubmissionServiceUrl.Current.Value,
            DisableSentryLogger = DisableSentryLogging.Current.Value,
            NonG0V0Server = NonG0V0Server.Current.Value,
        };

        File.WriteAllText(
            filePath,
            JsonConvert.SerializeObject(authlibRulesetConfig)
        );
        Notifications.Post(new ApiChangedNotification());
    }
}
