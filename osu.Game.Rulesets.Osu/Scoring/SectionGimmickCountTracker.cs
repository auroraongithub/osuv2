// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class SectionGimmickCountTracker
    {
        private int currentSectionId = -1;
        private int count300;
        private int count100;
        private int count50;
        private int countMiss;

        public void EnterSection(int sectionId)
        {
            if (currentSectionId == sectionId)
                return;

            currentSectionId = sectionId;
            count300 = 0;
            count100 = 0;
            count50 = 0;
            countMiss = 0;
        }

        public void Record(JudgementResult result, SectionGimmickSettings settings)
        {
            HitResult mapped = mapResultForCountLimit(result, settings);

            switch (mapped)
            {
                case HitResult.Great:
                    count300++;
                    break;

                case HitResult.Ok:
                    count100++;
                    break;

                case HitResult.Meh:
                    count50++;
                    break;

                case HitResult.Miss:
                    countMiss++;
                    break;
            }
        }

        public bool Exceeds(SectionGimmickSettings settings, JudgementResult result)
        {
            if (!settings.EnableCountLimits)
                return false;

            HitResult mapped = mapResultForCountLimit(result, settings);

            return mapped switch
            {
                HitResult.Great => settings.Max300s >= 0 && count300 > settings.Max300s,
                HitResult.Ok => settings.Max100s >= 0 && count100 > settings.Max100s,
                HitResult.Meh => settings.Max50s >= 0 && count50 > settings.Max50s,
                HitResult.Miss => settings.MaxMisses >= 0 && countMiss > settings.MaxMisses,
                _ => false
            };
        }

        private static HitResult mapResultForCountLimit(JudgementResult result, SectionGimmickSettings settings)
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
                    if (settings.Max300sAffectsSliderEndsAndTicks)
                        return HitResult.Great;
                    if (settings.Max100sAffectsSliderEndsAndTicks)
                        return HitResult.Ok;
                    if (settings.Max50sAffectsSliderEndsAndTicks)
                        return HitResult.Meh;
                    return HitResult.None;

                case HitResult.LargeTickMiss:
                case HitResult.SmallTickMiss:
                    return settings.MaxMissesAffectsSliderEndAndTickMisses ? HitResult.Miss : HitResult.None;

                case HitResult.IgnoreMiss:
                    return settings.MaxMissesAffectsSliderEndAndTickMisses && result.HitObject is SliderEndCircle
                        ? HitResult.Miss
                        : HitResult.None;

                default:
                    return HitResult.None;
            }
        }
    }
}
