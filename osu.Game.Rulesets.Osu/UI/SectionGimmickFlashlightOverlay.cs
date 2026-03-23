// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Adds a section-scoped Flashlight effect without requiring FL to be selected as a global gameplay mod.
    /// </summary>
    public partial class SectionGimmickFlashlightOverlay : CompositeDrawable
    {
        private readonly BeatmapSectionGimmicks gimmicks;
        private readonly SectionForcedFlashlightMod forcedFlashlightMod;

        private bool wasFlashlightForced;

        [Resolved(canBeNull: true)]
        private HealthProcessor? healthProcessor { get; set; }

        public SectionGimmickFlashlightOverlay(IBeatmap beatmap, DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            gimmicks = beatmap.SectionGimmicks;

            RelativeSizeAxes = Axes.Both;

            forcedFlashlightMod = new SectionForcedFlashlightMod();
            forcedFlashlightMod.ApplyToDrawableRuleset(drawableRuleset);
        }

        protected override void Update()
        {
            base.Update();

            bool isForced = isFlashlightForcedAtCurrentTime();

            if (isForced == wasFlashlightForced)
                return;

            wasFlashlightForced = isForced;
            forcedFlashlightMod.SetSectionForceActive(isForced);
        }

        private bool isFlashlightForcedAtCurrentTime()
        {
            if (healthProcessor is SectionGimmickHealthProcessor sectionHealthProcessor)
                return sectionHealthProcessor.ActiveSection?.Settings.ForceFlashlight == true;

            SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(gimmicks, Time.Current);
            return section?.Settings.ForceFlashlight == true;
        }

        public static bool HasAnyForcedFlashlightSection(IBeatmap beatmap)
            => beatmap.SectionGimmicks.Sections.Any(s => s.Settings.ForceFlashlight);

        private sealed class SectionForcedFlashlightMod : ModFlashlight<OsuHitObject>, IApplicableToDrawableHitObject
        {
            public override double ScoreMultiplier => 1;

            public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
            {
                MinValue = 1,
                MaxValue = 1,
            };

            public override BindableBool ComboBasedSize { get; } = new BindableBool(false);

            public override float DefaultFlashlightSize => 125;

            private SectionForcedFlashlight? flashlight;

            protected override Flashlight CreateFlashlight() => flashlight = new SectionForcedFlashlight(this);

            public void SetSectionForceActive(bool active)
                => flashlight?.SetSectionActive(active);

            public void ApplyToDrawableHitObject(DrawableHitObject drawable)
            {
                if (drawable is DrawableSlider slider)
                    slider.OnUpdate += _ => flashlight?.OnSliderTrackingChange(slider);
            }
        }

        private sealed partial class SectionForcedFlashlight : ModFlashlight<OsuHitObject>.Flashlight, IRequireHighFrequencyMousePosition
        {
            private const double follow_delay = 120;

            private bool sectionActive;

            public SectionForcedFlashlight(ModFlashlight modFlashlight)
                : base(modFlashlight)
            {
                FlashlightSize = new Vector2(0, GetSize());
                FlashlightSmoothness = 1.4f;
                Alpha = 0;
                Colour = Colour4.Black;
            }

            public void SetSectionActive(bool active)
            {
                if (sectionActive == active)
                    return;

                sectionActive = active;

                ClearTransforms(targetMember: nameof(Alpha));
                this.FadeTo(sectionActive ? 1 : 0, 120);

                if (!sectionActive)
                    FlashlightDim = 0;
            }

            public void OnSliderTrackingChange(DrawableSlider slider)
            {
                FlashlightDim = sectionActive && Time.Current >= slider.HitObject.StartTime && slider.Tracking.Value ? 0.8f : 0.0f;
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var position = FlashlightPosition;
                var destination = e.MousePosition;

                FlashlightPosition = Interpolation.ValueAt(
                    Math.Min(Math.Abs(Clock.ElapsedFrameTime), follow_delay), position, destination, 0, follow_delay, Easing.Out);

                return base.OnMouseMove(e);
            }

            protected override void UpdateFlashlightSize(float size)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, size), ModFlashlight<OsuHitObject>.FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
