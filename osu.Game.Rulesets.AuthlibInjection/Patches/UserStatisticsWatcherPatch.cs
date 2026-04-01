#nullable enable
using System;
using System.Collections.Generic;
using HarmonyLib;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Rulesets.AuthlibInjection.Configuration;
using osu.Game.Rulesets.AuthlibInjection.Extensions;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

public static class ScheduleAccess
{
    public static Func<Drawable, Action, ScheduledDelegate> ScheduleDelegate;

    static ScheduleAccess()
    {
        var mi = AccessTools.Method(
            typeof(Drawable),
            "Schedule",
            [typeof(Action)]
        );

        ScheduleDelegate = (Func<Drawable, Action, ScheduledDelegate>)Delegate.CreateDelegate(
            typeof(Func<Drawable, Action, ScheduledDelegate>),
            null,
            mi
        );
    }
}

[HarmonyPatch(typeof(UserStatisticsWatcher), "userScoreProcessed")]
public class UserStatisticsWatcherPatch
{
    static bool Prefix(UserStatisticsWatcher __instance, int userId, long scoreId)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return true;
        }

        var api = Traverse.Create(__instance).Property("api").GetValue<IAPIProvider>();
        var watchedScores = Traverse.Create(__instance).Field("watchedScores").GetValue<Dictionary<long, ScoreInfo>>();
        var statisticsProvider = Traverse.Create(__instance).Field("statisticsProvider").GetValue<LocalUserStatisticsProvider>();
        var latestUpdate = Traverse.Create(__instance).Field("latestUpdate").GetValue<Bindable<ScoreBasedUserStatisticsUpdate?>>();

        if (userId != api.LocalUser.Value?.OnlineID)
            return false;

        if (!watchedScores.Remove(scoreId, out var scoreInfo))
            return false;

        statisticsProvider.RefetchStatistics(scoreInfo, u =>
        {
            ScheduleAccess.ScheduleDelegate(__instance, () =>
            {
                if (u.OldStatistics != null)
                    latestUpdate.Value = new ScoreBasedUserStatisticsUpdate(scoreInfo, u.OldStatistics, u.NewStatistics);
            });
        });
        return false;
    }
}
