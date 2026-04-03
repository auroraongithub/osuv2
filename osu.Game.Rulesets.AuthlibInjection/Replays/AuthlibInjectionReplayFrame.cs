using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.AuthlibInjection.Replays
{
    public class AuthlibInjectionReplayFrame : ReplayFrame
    {
        public List<AuthlibInjectionAction> Actions = [];
        public Vector2 Position;

        public AuthlibInjectionReplayFrame(AuthlibInjectionAction? button = null)
        {
            if (button.HasValue)
                Actions.Add(button.Value);
        }

        public override bool IsEquivalentTo(ReplayFrame other)
            => other is AuthlibInjectionReplayFrame freeformFrame && Time == freeformFrame.Time &&
               Position == freeformFrame.Position && Actions.SequenceEqual(freeformFrame.Actions);
    }
}
