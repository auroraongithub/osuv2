using osu.Game.Beatmaps;
using osu.Game.Rulesets.AuthlibInjection.Objects;

namespace osu.Game.Rulesets.AuthlibInjection.Beatmaps
{
    public class AuthlibInjectionBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
        : BeatmapConverter<AuthlibInjectionHitObject>(beatmap, ruleset)
    {
        public override bool CanConvert() => false;
    }
}
