using HarmonyLib;
using osu.Framework.IO.Network;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.AuthlibInjection.Configuration;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(CreateRoomScoreRequest), "CreateWebRequest")]
[HarmonyPriority(Priority.Low)]
public class CreateRoomScoreRequestPatch
{
    static void Postfix(CreateRoomScoreRequest __instance, ref WebRequest __result)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return;
        }

        GlobalConfigManager.InitializeHashCache();

        int rulesetId = Traverse.Create(__instance).Field("rulesetId").GetValue<int>();
        string rulesetHash = GlobalConfigManager.HashCache.GetHash(rulesetId);

        if (rulesetHash != null)
        {
            __result.AddParameter("ruleset_hash", rulesetHash);
        }
    }
}
