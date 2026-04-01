using System.Linq;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Rulesets.AuthlibInjection.Extensions;

public static class TabControlExtension
{
    public static void AddItemIfNonExist<T>(this TabControl<T> tabControl, T item)
    {
        if (!tabControl.Items.Contains(item))
        {
            tabControl.AddItem(item);
        }
    }
}
