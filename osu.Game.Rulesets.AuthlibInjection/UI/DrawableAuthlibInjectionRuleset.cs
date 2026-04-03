using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.AuthlibInjection.Objects;
using osu.Game.Rulesets.AuthlibInjection.Objects.Drawables;
using osu.Game.Rulesets.AuthlibInjection.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.AuthlibInjection.UI
{
    [Cached]
    public partial class DrawableAuthlibInjectionRuleset(
        AuthlibInjectionRuleset ruleset,
        IBeatmap beatmap,
        IReadOnlyList<Mod> mods = null)
        : DrawableRuleset<AuthlibInjectionHitObject>(ruleset, beatmap, mods)
    {
        protected override Playfield CreatePlayfield() => new AuthlibInjectionPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) =>
            new AuthlibInjectionFramedReplayInputHandler(replay);

        public override DrawableHitObject<AuthlibInjectionHitObject>
            CreateDrawableRepresentation(AuthlibInjectionHitObject h) => new DrawableAuthlibInjectionHitObject(h);

        protected override PassThroughInputManager CreateInputManager() =>
            new AuthlibInjectionInputManager(Ruleset?.RulesetInfo);
    }
}
