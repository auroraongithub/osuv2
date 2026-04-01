using HarmonyLib;
using osu.Game.Online;
using osu.Game.Rulesets.AuthlibInjection.Configuration;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(TrustedDomainOnlineStore), "GetLookupUrl")]
public class TrustedDomainPatch
{
    static bool Prefix(ref string __result, string url)
    {
        if (!GlobalConfigManager.Patched)
        {
            return true;
        }

        __result = url;
        return false;
    }
}
