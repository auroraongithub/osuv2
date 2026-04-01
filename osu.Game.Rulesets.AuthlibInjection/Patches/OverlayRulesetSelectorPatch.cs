using System.Linq;
using HarmonyLib;
using osu.Game.Extensions;
using osu.Game.Overlays;
using osu.Game.Rulesets.AuthlibInjection.Configuration;
using osu.Game.Rulesets.AuthlibInjection.Extensions;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(RulesetSelector), "load")]
public class OverlayRulesetSelectorPatch
{
    private static void addRulesets(RulesetSelector rulesetSelector)
    {
        var rulesets = Traverse.Create(rulesetSelector).Property("Rulesets").GetValue<RulesetStore>();

        foreach (var ruleset in rulesets.AvailableRulesets.Where(r => r.IsLegacyRuleset()))
        {
            rulesetSelector.AddItemIfNonExist(ruleset);

            if (rulesetSelector is not OverlayRulesetSelector)
            {
                continue;
            }

            switch (ruleset.ShortName)
            {
                case RulesetInfoExtension.OSU_MODE_SHORTNAME:
                    rulesetSelector.AddItemIfNonExist(ruleset.CreateSpecialRuleset(RulesetInfoExtension.OSU_RELAX_MODE_SHORTNAME, RulesetInfoExtension.OSU_RELAX_ONLINE_ID));
                    rulesetSelector.AddItemIfNonExist(ruleset.CreateSpecialRuleset(RulesetInfoExtension.OSU_AUTOPILOT_MODE_SHORTNAME, RulesetInfoExtension.OSU_AUTOPILOT_ONLINE_ID));
                    break;

                case RulesetInfoExtension.TAIKO_MODE_SHORTNAME:
                    rulesetSelector.AddItemIfNonExist(ruleset.CreateSpecialRuleset(RulesetInfoExtension.TAIKO_RELAX_MODE_SHORTNAME, RulesetInfoExtension.TAIKO_RELAX_ONLINE_ID));
                    break;

                case RulesetInfoExtension.CATCH_MODE_SHORTNAME:
                    rulesetSelector.AddItemIfNonExist(ruleset.CreateSpecialRuleset(RulesetInfoExtension.CATCH_RELAX_MODE_SHORTNAME, RulesetInfoExtension.CATCH_RELAX_ONLINE_ID));
                    break;
            }
        }
    }

    static bool Prefix(RulesetSelector __instance)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return true;
        }

        addRulesets(__instance);
        return false;
    }
}
