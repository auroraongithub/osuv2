using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.AuthlibInjection.Replays
{
    public class AuthlibInjectionFramedReplayInputHandler(Replay replay)
        : FramedReplayInputHandler<AuthlibInjectionReplayFrame>(replay)
    {
        protected override bool IsImportant(AuthlibInjectionReplayFrame frame) => frame.Actions.Any();

        protected override void CollectReplayInputs(List<IInput> inputs)
        {
        }
    }
}
