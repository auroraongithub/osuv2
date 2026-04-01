using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.AuthlibInjection.Configuration;

namespace osu.Game.Rulesets.AuthlibInjection.Notifications;

public partial class CurrentServerNotification : SimpleNotification
{
    private static Tuple<string, string> determineServer(EndpointConfiguration endpointConfiguration)
    {
        string apiUrl = string.IsNullOrEmpty(GlobalConfigManager.Config.ApiUrl) ? endpointConfiguration.APIUrl : GlobalConfigManager.Config.ApiUrl;
        string websiteUrl = string.IsNullOrEmpty(GlobalConfigManager.Config.WebsiteUrl) ? endpointConfiguration.WebsiteUrl : GlobalConfigManager.Config.WebsiteUrl;
        return new Tuple<string, string>(apiUrl, websiteUrl);
    }

    [BackgroundDependencyLoader]
    private void load(OsuColour colours, OsuGame game)
    {
        var urls = determineServer(game.CreateEndpoints());
        string server = GlobalConfigManager.IsOfficialServer(urls.Item1) || string.IsNullOrEmpty(urls.Item1) ? $"Official ({urls.Item1})" : urls.Item1;
        Text = $"Current server: {server}";
        Icon = FontAwesome.Solid.Server;
        IconContent.Colour = colours.BlueDark;
        Activated = () =>
        {
            game.OpenUrlExternally(urls.Item2, LinkWarnMode.AlwaysWarn);
            return true;
        };
    }
}
