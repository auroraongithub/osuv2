// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.AuthlibInjection.UI
{
    [Cached]
    public partial class AuthlibInjectionPlayfield : Playfield
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal([
                HitObjectContainer
            ]);
        }
    }
}
