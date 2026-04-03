using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Rulesets.AuthlibInjection.Notifications;

public partial class ApiChangedNotification : SimpleNotification
{
    public ApiChangedNotification()
    {
        Text = "API settings changed, please restart the game to apply changes.";
    }

    [BackgroundDependencyLoader]
    private void load(OsuColour colours)
    {
        Icon = FontAwesome.Solid.Server;
        IconContent.Colour = colours.BlueDark;
    }
}
