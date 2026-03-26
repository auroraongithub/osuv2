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
using osu.Game.Beatmaps.HitObjectGimmicks;
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
        private readonly BeatmapHitObjectGimmicks hitObjectGimmicks;
        private readonly SectionForcedFlashlightMod forcedFlashlightMod;

        private bool wasFlashlightForced;
        private float? lastRadius;

        [Resolved(canBeNull: true)]
        private HealthProcessor? healthProcessor { get; set; }

        public SectionGimmickFlashlightOverlay(IBeatmap beatmap, DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            gimmicks = beatmap.SectionGimmicks;
            hitObjectGimmicks = beatmap.HitObjectGimmicks;

            RelativeSizeAxes = Axes.Both;

            forcedFlashlightMod = new SectionForcedFlashlightMod();
            forcedFlashlightMod.ApplyToDrawableRuleset(drawableRuleset);
        }

        protected override void Update()
        {
            base.Update();

            bool isForced = isFlashlightForcedAtCurrentTime();
            float? radius = getFlashlightRadiusAtCurrentTime();
            float fade = getFlashlightFadeAtCurrentTime(isForced);

            if (isForced == wasFlashlightForced && nullableFloatEquals(radius, lastRadius) && Math.Abs(fade - forcedFlashlightMod.CurrentSectionFade) <= 0.001f)
                return;

            wasFlashlightForced = isForced;
            lastRadius = radius;
            forcedFlashlightMod.SetSectionForceActive(isForced);
            forcedFlashlightMod.SetSectionRadius(radius);
            forcedFlashlightMod.SetSectionFade(fade);
        }

        private bool isFlashlightForcedAtCurrentTime()
        {
            if (healthProcessor is SectionGimmickHealthProcessor sectionHealthProcessor)
            {
                if (sectionHealthProcessor.ActiveSection?.Settings.ForceFlashlight == true)
                    return true;
            }

            SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(gimmicks, Time.Current);
            if (section?.Settings.ForceFlashlight == true)
                return true;

            return hitObjectGimmicks.Entries.Any(e => e.Settings?.ForceFlashlight == true && Math.Abs(e.StartTime - Time.Current) <= 1);
        }

        private float? getFlashlightRadiusAtCurrentTime()
        {
            SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(gimmicks, Time.Current);

            if (section?.Settings.ForceFlashlight == true)
            {
                float sectionRadius = computeSectionRadius(section, Time.Current);
                if (!float.IsNaN(sectionRadius))
                    return Math.Clamp(sectionRadius, 20f, 400f);
            }

            var entry = hitObjectGimmicks.Entries
                .Where(e => e.Settings?.ForceFlashlight == true && Math.Abs(e.StartTime - Time.Current) <= 1)
                .LastOrDefault();

            if (entry?.Settings != null && !float.IsNaN(entry.Settings.FlashlightRadius))
                return Math.Clamp(entry.Settings.FlashlightRadius, 20f, 400f);

            return null;
        }

        private float getFlashlightFadeAtCurrentTime(bool isForced)
        {
            if (!isForced)
                return 0;

            SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(gimmicks, Time.Current);
            if (section == null || !section.Settings.ForceFlashlight)
                return 1;

            var settings = section.Settings;

            if (!settings.EnableGradualFlashlightFadeIn)
                return 1;

            double sectionEnd = section.EndTime >= 0 ? section.EndTime : double.MaxValue;
            double gradualEnd = float.IsNaN(settings.GradualFlashlightRadiusEndTimeMs) ? sectionEnd : settings.GradualFlashlightRadiusEndTimeMs;

            if (gradualEnd > sectionEnd)
                gradualEnd = sectionEnd;

            if (gradualEnd <= section.StartTime)
                return 1;

            return (float)Math.Clamp((Time.Current - section.StartTime) / (gradualEnd - section.StartTime), 0, 1);
        }

        private static float computeSectionRadius(SectionGimmickSection section, double currentTime)
        {
            var settings = section.Settings;

            if (float.IsNaN(settings.FlashlightRadius))
                return float.NaN;

            if (!settings.EnableGradualFlashlightRadiusChange)
                return settings.FlashlightRadius;

            double sectionEnd = section.EndTime >= 0 ? section.EndTime : double.MaxValue;
            double gradualEnd = float.IsNaN(settings.GradualFlashlightRadiusEndTimeMs) ? sectionEnd : settings.GradualFlashlightRadiusEndTimeMs;

            if (gradualEnd > sectionEnd)
                gradualEnd = sectionEnd;

            if (gradualEnd <= section.StartTime)
                return settings.FlashlightRadius;

            double progress = Math.Clamp((currentTime - section.StartTime) / (gradualEnd - section.StartTime), 0, 1);
            const float defaultRadius = 125f;
            const float shrinkStartRadius = 400f;
            float startRadius = settings.EnableGradualFlashlightRadiusChange ? shrinkStartRadius : defaultRadius;

            return (float)(startRadius + (settings.FlashlightRadius - startRadius) * progress);
        }

        private static bool nullableFloatEquals(float? a, float? b)
        {
            if (!a.HasValue && !b.HasValue)
                return true;

            if (!a.HasValue || !b.HasValue)
                return false;

            return Math.Abs(a.Value - b.Value) <= 0.001f;
        }

        public static bool HasAnyForcedFlashlightSection(IBeatmap beatmap)
            => beatmap.SectionGimmicks.Sections.Any(s => s.Settings.ForceFlashlight)
               || beatmap.HitObjectGimmicks.Entries.Any(e => e.Settings?.ForceFlashlight == true);

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

            public void SetSectionRadius(float? radius)
                => flashlight?.SetSectionRadius(radius);

            public void SetSectionFade(float fade)
                => flashlight?.SetSectionFade(fade);

            public float CurrentSectionFade => flashlight?.CurrentSectionFade ?? 0;

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
            private float? sectionRadius;
            private float sectionFade = 1;
            public float CurrentSectionFade => Alpha;

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

                sectionFade = sectionActive ? 1 : 0;

                if (!sectionActive)
                    FlashlightDim = 0;

                if (sectionActive)
                    applyRadius();
            }

            public void SetSectionRadius(float? radius)
            {
                sectionRadius = radius;

                if (sectionActive)
                    applyRadius();
            }

            public void SetSectionFade(float fade)
            {
                if (!sectionActive)
                    return;

                sectionFade = Math.Clamp(fade, 0, 1);
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
                float target = sectionActive && sectionRadius.HasValue ? sectionRadius.Value : size;
                FlashlightSize = new Vector2(0, target);
            }

            private void applyRadius()
            {
                float target = sectionRadius ?? GetSize();
                FlashlightSize = new Vector2(0, target);
            }

            protected override void Update()
            {
                base.Update();
                Alpha = sectionActive ? sectionFade : 0;
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
