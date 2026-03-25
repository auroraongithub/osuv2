// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class SectionGimmickHealthProcessor : OsuHealthProcessor
    {
        private readonly SectionGimmickCountTracker countTracker = new SectionGimmickCountTracker();
        private BeatmapSectionGimmicks gimmicks = new BeatmapSectionGimmicks();
        private SectionGimmickSection? activeSection;

        public SectionGimmickSection? ActiveSection => activeSection;

        public SectionGimmickHealthProcessor(double drainStartTime)
            : base(drainStartTime)
        {
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            gimmicks = beatmap.SectionGimmicks ?? new BeatmapSectionGimmicks();
            SectionGimmicksValidator.Validate(gimmicks);
            base.ApplyBeatmap(beatmap);
        }

        protected override void Update()
        {
            base.Update();

            var section = resolveSection(Time.Current);
            if (section == null)
                return;

            var settings = section.Settings;

            if (settings.EnableHPGimmick && !float.IsNaN(settings.HPCap))
                Health.Value = Math.Min(Health.Value, settings.HPCap);

            if ((settings.EnableHPGimmick && settings.NoDrain) || settings.EnableGreatOffsetPenalty)
            {
                // cancel out frame drain while this mode is active.
                // this intentionally keeps behaviour section-scoped.
                Health.Value += DrainRate * Time.Elapsed;
            }
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            var section = resolveSection(result.HitObject.StartTime);
            if (section != null)
            {
                var settings = section.Settings;

                if (settings.EnableNoMiss && result.Type == HitResult.Miss)
                {
                    TriggerFailure();
                    return;
                }

                if (settings.EnableNoMissedSliderEnd &&
                    (result.HitObject is SliderEndCircle || result.HitObject is SliderTick) &&
                    (result.Type == HitResult.IgnoreMiss || result.Type == HitResult.LargeTickMiss || result.Type == HitResult.SmallTickMiss))
                {
                    TriggerFailure();
                    return;
                }

                countTracker.Record(result, settings);
                if (countTracker.Exceeds(settings, result))
                {
                    TriggerFailure();
                    return;
                }
            }

            base.ApplyResultInternal(result);

            // Apply additional offset penalty after base judgement application.
            if (section != null)
            {
                var settings = section.Settings;
                if (settings.EnableGreatOffsetPenalty && shouldApplyOffsetPenalty(settings, result))
                {
                    if (Math.Abs(result.TimeOffset) > settings.GreatOffsetThresholdMs)
                        Health.Value += settings.GreatOffsetPenaltyHP;
                }
            }
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
        {
            var section = resolveSection(result.HitObject.StartTime);
            if (section == null)
                return base.GetHealthIncreaseFor(result);

            var settings = section.Settings;
            if (!settings.EnableHPGimmick)
                return base.GetHealthIncreaseFor(result);

            float hpValue = mapResultForHp(result, settings) switch
            {
                HitResult.Great => settings.HP300,
                HitResult.Ok => settings.HP100,
                HitResult.Meh => settings.HP50,
                HitResult.Miss => settings.HPMiss,
                _ => float.NaN
            };

            if (float.IsNaN(hpValue))
                return base.GetHealthIncreaseFor(result);

            // When ReverseHP is false, positive HP values should drain (subtract from health)
            // When ReverseHP is true, positive HP values should heal (add to health)
            double delta = settings.ReverseHP ? hpValue : -hpValue;

            if (!float.IsNaN(settings.HPCap))
                delta = Math.Min(delta, settings.HPCap - Health.Value);

            return delta;
        }

        private static HitResult mapResultForHp(JudgementResult result, SectionGimmickSettings settings)
        {
            switch (result.Type)
            {
                case HitResult.Great:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return result.Type;

                case HitResult.LargeTickHit:
                case HitResult.SmallTickHit:
                case HitResult.SliderTailHit:
                    if (settings.HP300AffectsSliderEndsAndTicks)
                        return HitResult.Great;
                    if (settings.HP100AffectsSliderEndsAndTicks)
                        return HitResult.Ok;
                    if (settings.HP50AffectsSliderEndsAndTicks)
                        return HitResult.Meh;
                    return HitResult.None;

                case HitResult.LargeTickMiss:
                case HitResult.SmallTickMiss:
                    return settings.HPMissAffectsSliderEndAndTickMisses ? HitResult.Miss : HitResult.None;

                case HitResult.IgnoreMiss:
                    return settings.HPMissAffectsSliderEndAndTickMisses && result.HitObject is SliderEndCircle
                        ? HitResult.Miss
                        : HitResult.None;

                default:
                    return HitResult.None;
            }
        }

        private SectionGimmickSection? resolveSection(double time)
        {
            var section = SectionGimmickSectionResolver.Resolve(gimmicks, time);

            if (section == null)
            {
                activeSection = null;
                return null;
            }

            if (activeSection?.Id != section.Id)
            {
                activeSection = section;
                countTracker.EnterSection(section.Id);

                var settings = section.Settings;

                if (settings.EnableHPGimmick && !float.IsNaN(settings.HPStart))
                    Health.Value = settings.HPStart;

                if ((settings.EnableHPGimmick && settings.NoDrain && !settings.ReverseHP) && settings.EnableGreatOffsetPenalty)
                {
                    // both enabled: no-op special case, handled by per-hit logic.
                }
            }

            return section;
        }

        private static bool shouldApplyOffsetPenalty(SectionGimmickSettings settings, JudgementResult result)
        {
            bool baseApplicable = result.Type is HitResult.Great or HitResult.Ok or HitResult.Meh;
            if (!baseApplicable)
                return false;

            // If HP gimmick already provides explicit per-judgement punishment for this result,
            // avoid double-penalising that same judgement type.
            if (!settings.EnableHPGimmick)
                return true;

            float configured = result.Type switch
            {
                HitResult.Great => settings.HP300,
                HitResult.Ok => settings.HP100,
                HitResult.Meh => settings.HP50,
                _ => float.NaN,
            };

            if (float.IsNaN(configured))
                return true;

            double resultingHealthDelta = settings.ReverseHP ? configured : -configured;
            bool hpGimmickAlreadyPunishes = resultingHealthDelta < 0;
            return !hpGimmickAlreadyPunishes;
        }
    }
}
