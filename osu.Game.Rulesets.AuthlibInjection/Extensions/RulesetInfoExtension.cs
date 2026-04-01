#nullable enable
using System;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.AuthlibInjection.Extensions;

public static class RulesetInfoExtension
{
    #region Constants

    public const string OSU_MODE_SHORTNAME = "osu";
    public const string TAIKO_MODE_SHORTNAME = "taiko";
    public const string CATCH_MODE_SHORTNAME = "fruits";

    // https://github.com/GooGuTeam/g0v0-server/blob/main/README.en.md#supported-rulesets
    public const string OSU_RELAX_MODE_SHORTNAME = "osurx";
    public const string OSU_AUTOPILOT_MODE_SHORTNAME = "osuap";
    public const string TAIKO_RELAX_MODE_SHORTNAME = "taikorx";
    public const string CATCH_RELAX_MODE_SHORTNAME = "fruitsrx";

    public const int OSU_RELAX_ONLINE_ID = 4;
    public const int OSU_AUTOPILOT_ONLINE_ID = 5;
    public const int TAIKO_RELAX_ONLINE_ID = 6;
    public const int CATCH_RELAX_ONLINE_ID = 7;

    #endregion

    #region ModelExtension from GooGuTeam/osu

    /// <summary>
    /// Check whether this <see cref="IRulesetInfo"/> represents a special ruleset (ie. any of the relax or autopilot modes).
    /// </summary>
    public static bool IsSpecialRuleset(this IRulesetInfo ruleset) => ruleset.ShortName is OSU_RELAX_MODE_SHORTNAME or OSU_AUTOPILOT_MODE_SHORTNAME
        or TAIKO_RELAX_MODE_SHORTNAME or CATCH_RELAX_MODE_SHORTNAME;

    /// <summary>
    /// Check whether this <see cref="IRulesetInfo"/> has special rulesets associated with it (ie. is either osu!, osu!taiko, or osu!catch).
    /// </summary>
    public static bool HasSpecialRuleset(this IRulesetInfo ruleset) => ruleset.ShortName is OSU_MODE_SHORTNAME or TAIKO_MODE_SHORTNAME or CATCH_MODE_SHORTNAME;

    #endregion

    #region RulesetInfo from GooGuTeam/osu

    /// <summary>
    /// Create a special ruleset based on a normal <see cref="RulesetInfo"/>.
    /// </summary>
    public static RulesetInfo CreateSpecialRuleset(this RulesetInfo ruleset, string newShortName, int onlineId)
    {
        string suffix = newShortName[^2..].ToUpperInvariant();

        var newRuleset = ruleset.Clone();
        newRuleset.OnlineID = onlineId;
        newRuleset.ShortName = newShortName;
        newRuleset.Name = $"{newRuleset.Name} ({suffix})";
        return newRuleset;
    }

    /// <summary>
    /// Create a normal ruleset based on a special <see cref="RulesetInfo"/>.
    /// </summary>
    public static RulesetInfo CreateNormalRuleset(this RulesetInfo ruleset)
    {
        string baseShortName = ruleset.ShortName.Length > 4 ? ruleset.ShortName[..^2] : ruleset.ShortName;

        var newRuleset = ruleset.Clone();
        newRuleset.OnlineID = ruleset.OnlineID switch
        {
            OSU_RELAX_ONLINE_ID or OSU_AUTOPILOT_ONLINE_ID => 0,
            TAIKO_RELAX_ONLINE_ID => 1,
            CATCH_RELAX_ONLINE_ID => 2,
            _ => ruleset.OnlineID,
        };
        newRuleset.ShortName = baseShortName;
        newRuleset.Name = newRuleset.Name.Contains('(')
            ? newRuleset.Name[..newRuleset.Name.LastIndexOf(" (", StringComparison.Ordinal)]
            : newRuleset.Name;
        return newRuleset;
    }

    /// <summary>
    /// Create a special ruleset based on the mods applied to a <see cref="ScoreInfo"/>.
    /// </summary>
    public static RulesetInfo? CreateSpecialRulesetByScore(this ScoreInfo score)
    {
        if (!score.Ruleset.HasSpecialRuleset()) { return null; }

        return score.Ruleset.ShortName switch
        {
            OSU_MODE_SHORTNAME when score.Mods.OfType<ModRelax>().Any() => score.Ruleset.CreateSpecialRuleset(OSU_RELAX_MODE_SHORTNAME, OSU_RELAX_ONLINE_ID),
            OSU_MODE_SHORTNAME when score.APIMods.Any(m => m.Acronym == "AP") => score.Ruleset.CreateSpecialRuleset(OSU_AUTOPILOT_MODE_SHORTNAME, OSU_AUTOPILOT_ONLINE_ID),
            TAIKO_MODE_SHORTNAME when score.Mods.OfType<ModRelax>().Any() => score.Ruleset.CreateSpecialRuleset(TAIKO_RELAX_MODE_SHORTNAME, TAIKO_RELAX_ONLINE_ID),
            CATCH_MODE_SHORTNAME when score.Mods.OfType<ModRelax>().Any() => score.Ruleset.CreateSpecialRuleset(CATCH_RELAX_MODE_SHORTNAME, CATCH_RELAX_ONLINE_ID),
            _ => null
        };
    }

    #endregion
}
