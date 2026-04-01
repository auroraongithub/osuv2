#nullable enable
using System;
using osu.Game.Online;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.AuthlibInjection.Extensions;

public static class LocalUserStatisticsProviderExtension
{
    public static void RefetchStatistics(this LocalUserStatisticsProvider localUserStatisticsProvider, ScoreInfo score, Action<UserStatisticsUpdate>? callback = null)
    {
        var specialRuleset = score.CreateSpecialRulesetByScore();
        localUserStatisticsProvider.RefetchStatistics(specialRuleset ?? score.Ruleset, callback);
    }
}
