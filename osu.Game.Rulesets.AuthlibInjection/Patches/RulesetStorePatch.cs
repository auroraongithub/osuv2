using System;
using System.Linq;
using HarmonyLib;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.AuthlibInjection.Configuration;
using osu.Game.Rulesets.AuthlibInjection.Extensions;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(RulesetStore), nameof(RulesetStore.GetRuleset), typeof(string))]
public class RulesetStorePatch
{
    static bool Prefix(RulesetStore __instance, string shortName, ref IRulesetInfo __result)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return true;
        }

        try
        {
            if (shortName is RulesetInfoExtension.OSU_RELAX_MODE_SHORTNAME or RulesetInfoExtension.OSU_AUTOPILOT_MODE_SHORTNAME or RulesetInfoExtension.TAIKO_RELAX_MODE_SHORTNAME
                or RulesetInfoExtension.CATCH_RELAX_MODE_SHORTNAME)
            {
                string baseShortName = shortName switch
                {
                    RulesetInfoExtension.OSU_RELAX_MODE_SHORTNAME => RulesetInfoExtension.OSU_MODE_SHORTNAME,
                    RulesetInfoExtension.OSU_AUTOPILOT_MODE_SHORTNAME => RulesetInfoExtension.OSU_MODE_SHORTNAME,
                    RulesetInfoExtension.TAIKO_RELAX_MODE_SHORTNAME => RulesetInfoExtension.TAIKO_MODE_SHORTNAME,
                    RulesetInfoExtension.CATCH_RELAX_MODE_SHORTNAME => RulesetInfoExtension.CATCH_MODE_SHORTNAME,
                    _ => throw new ArgumentOutOfRangeException()
                };
                int onlineId = shortName switch
                {
                    RulesetInfoExtension.OSU_RELAX_MODE_SHORTNAME => RulesetInfoExtension.OSU_RELAX_ONLINE_ID,
                    RulesetInfoExtension.OSU_AUTOPILOT_MODE_SHORTNAME => RulesetInfoExtension.OSU_AUTOPILOT_ONLINE_ID,
                    RulesetInfoExtension.TAIKO_RELAX_MODE_SHORTNAME => RulesetInfoExtension.TAIKO_RELAX_ONLINE_ID,
                    RulesetInfoExtension.CATCH_RELAX_MODE_SHORTNAME => RulesetInfoExtension.CATCH_RELAX_ONLINE_ID,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var baseRuleset = __instance.AvailableRulesets.FirstOrDefault((Func<RulesetInfo, bool>)(r => r.ShortName == baseShortName));
                __result = baseRuleset.AsNonNull().CreateSpecialRuleset(shortName, onlineId);
                return false;
            }

            __result = __instance.AvailableRulesets.FirstOrDefault((Func<RulesetInfo, bool>)(r => r.ShortName == shortName));

            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }
}
