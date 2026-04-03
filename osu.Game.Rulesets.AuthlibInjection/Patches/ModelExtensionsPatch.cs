using HarmonyLib;
using osu.Game.Extensions;
using osu.Game.Rulesets.AuthlibInjection.Configuration;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(ModelExtensions), nameof(ModelExtensions.IsLegacyRuleset))]
[HarmonyPriority(Priority.High)]
public class ModelExtensionsPatch
{
    static bool Prefix(IRulesetInfo ruleset, ref bool __result)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return true;
        }

        __result = ruleset.OnlineID >= 0;
        return false;
    }
}
