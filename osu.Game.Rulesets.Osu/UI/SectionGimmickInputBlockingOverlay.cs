// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class SectionGimmickInputBlockingOverlay : CompositeDrawable
    {
        private readonly BeatmapSectionGimmicks gimmicks;
        private readonly SectionForcedInputBlockingMod forcedInputBlockingMod;

        private ForcedInputMode lastMode;

        public SectionGimmickInputBlockingOverlay(IBeatmap beatmap, DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            gimmicks = beatmap.SectionGimmicks;

            RelativeSizeAxes = Axes.Both;

            forcedInputBlockingMod = new SectionForcedInputBlockingMod();
            forcedInputBlockingMod.ApplyToDrawableRuleset(drawableRuleset);
        }

        protected override void Update()
        {
            base.Update();

            ForcedInputMode mode = resolveMode(Time.Current);
            if (mode == lastMode)
                return;

            lastMode = mode;
            forcedInputBlockingMod.SetMode(mode);
        }

        private ForcedInputMode resolveMode(double time)
        {
            SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(gimmicks, time);
            if (section == null)
                return ForcedInputMode.None;

            if (section.Settings.ForceSingleTap)
                return ForcedInputMode.SingleTap;

            if (section.Settings.ForceAlternate)
                return ForcedInputMode.Alternate;

            return ForcedInputMode.None;
        }

        public static bool HasAnyForcedInputBlockingSection(IBeatmap beatmap)
            => beatmap.SectionGimmicks.Sections.Any(s => s.Settings.ForceSingleTap || s.Settings.ForceAlternate);

        private enum ForcedInputMode
        {
            None,
            SingleTap,
            Alternate,
        }

        private sealed class SectionForcedInputBlockingMod : InputBlockingMod
        {
            public override string Name => "Section Input Blocking";

            public override string Acronym => "SIB";

            public override LocalisableString Description => "applies section single tap / alternate behaviour";

            public override bool Ranked => false;

            private ForcedInputMode mode;

            public void SetMode(ForcedInputMode mode)
            {
                if (this.mode == mode)
                    return;

                this.mode = mode;
                ResetInputState();
            }

            protected override bool CheckValidNewAction(OsuAction action)
                => mode switch
                {
                    ForcedInputMode.None => true,
                    ForcedInputMode.SingleTap => LastAcceptedAction == null || LastAcceptedAction == action,
                    ForcedInputMode.Alternate => LastAcceptedAction != action,
                    _ => true,
                };
        }
    }
}
