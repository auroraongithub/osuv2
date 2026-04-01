using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using osu.Game.Extensions;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Rulesets.AuthlibInjection.Configuration;
using osu.Game.Rulesets.AuthlibInjection.Extensions;
using osu.Game.Users;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(LocalUserStatisticsProvider), "initialiseStatistics")]
public class LocalUserStatisticsProviderPatch
{
    static bool Prefix(LocalUserStatisticsProvider __instance)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return true;
        }

        var statisticsCache = Traverse.Create(__instance).Field("statisticsCache").GetValue<Dictionary<string, UserStatistics>>();
        var rulesets = Traverse.Create(__instance).Property("rulesets").GetValue<RulesetStore>();
        var api = Traverse.Create(__instance).Property("api").GetValue<IAPIProvider>();

        statisticsCache.Clear();

        if (api.LocalUser.Value == null || api.LocalUser.Value.Id <= 1)
            return false;

        foreach (var ruleset in rulesets.AvailableRulesets.Where(r => r.IsLegacyRuleset()))
        {
            __instance.RefetchStatistics(ruleset);

            switch (ruleset.ShortName)
            {
                case RulesetInfoExtension.OSU_MODE_SHORTNAME:
                    __instance.RefetchStatistics(ruleset.CreateSpecialRuleset(RulesetInfoExtension.OSU_RELAX_MODE_SHORTNAME, RulesetInfoExtension.OSU_RELAX_ONLINE_ID));
                    __instance.RefetchStatistics(ruleset.CreateSpecialRuleset(RulesetInfoExtension.OSU_AUTOPILOT_MODE_SHORTNAME, RulesetInfoExtension.OSU_AUTOPILOT_ONLINE_ID));
                    break;

                case RulesetInfoExtension.TAIKO_MODE_SHORTNAME:
                    __instance.RefetchStatistics(ruleset.CreateSpecialRuleset(RulesetInfoExtension.TAIKO_RELAX_MODE_SHORTNAME, RulesetInfoExtension.TAIKO_RELAX_ONLINE_ID));
                    break;

                case RulesetInfoExtension.CATCH_MODE_SHORTNAME:
                    __instance.RefetchStatistics(ruleset.CreateSpecialRuleset(RulesetInfoExtension.CATCH_RELAX_MODE_SHORTNAME, RulesetInfoExtension.CATCH_RELAX_ONLINE_ID));
                    break;
            }
        }

        return false;
    }
}
