using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.AuthlibInjection
{
    public partial class AuthlibInjectionInputManager(RulesetInfo ruleset)
        : RulesetInputManager<AuthlibInjectionAction>(ruleset, 0, SimultaneousBindingMode.Unique);

    public enum AuthlibInjectionAction;
}
