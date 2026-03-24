// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;

namespace osu.Game.Beatmaps.SectionGimmicks
{
    public static class SectionGimmicksValidator
    {
        public static void Validate(BeatmapSectionGimmicks gimmicks)
        {
            var ordered = gimmicks.Sections.OrderBy(s => s.StartTime).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var section = ordered[i];
                var settings = section.Settings;

                if (section.EndTime >= 0 && section.StartTime >= section.EndTime)
                    throw new InvalidOperationException($"Section {section.Id} has invalid range.");

                if (settings.Max300s < -1 || settings.Max100s < -1 || settings.Max50s < -1 || settings.MaxMisses < -1)
                    throw new InvalidOperationException($"Section {section.Id} has invalid max counts.");

                validateRange(settings.HP300, section.Id, nameof(settings.HP300));
                validateRange(settings.HP100, section.Id, nameof(settings.HP100));
                validateRange(settings.HP50, section.Id, nameof(settings.HP50));
                validateRange(settings.HPMiss, section.Id, nameof(settings.HPMiss));

                if (settings.EnableHPGimmick)
                {
                    if (!settings.NoDrain)
                        throw new InvalidOperationException($"Section {section.Id}: NoDrain is required for HP gimmick.");

                    requireNotNaN(settings.HP300, section.Id, nameof(settings.HP300));
                    requireNotNaN(settings.HP100, section.Id, nameof(settings.HP100));
                    requireNotNaN(settings.HP50, section.Id, nameof(settings.HP50));
                    requireNotNaN(settings.HPMiss, section.Id, nameof(settings.HPMiss));
                }

                if (settings.EnableGreatOffsetPenalty)
                {
                    if (settings.GreatOffsetThresholdMs < 0)
                        throw new InvalidOperationException($"Section {section.Id}: GreatOffsetThresholdMs must be >= 0.");

                    if (float.IsNaN(settings.GreatOffsetPenaltyHP) || settings.GreatOffsetPenaltyHP > 0)
                        throw new InvalidOperationException($"Section {section.Id}: GreatOffsetPenaltyHP must be <= 0.");
                }

                if (settings.ReverseHP)
                {
                    if (!settings.EnableHPGimmick)
                        throw new InvalidOperationException($"Section {section.Id}: ReverseHP requires EnableHPGimmick.");

                    if (!(settings.HP100 > 0 && settings.HP50 > 0 && settings.HPMiss > 0))
                        throw new InvalidOperationException($"Section {section.Id}: ReverseHP requires HP100/HP50/HPMiss > 0.");
                }

                if (settings.EnableDifficultyOverrides)
                {
                    if (float.IsNaN(settings.SectionCircleSize) && float.IsNaN(settings.SectionApproachRate) && float.IsNaN(settings.SectionOverallDifficulty))
                        throw new InvalidOperationException($"Section {section.Id}: difficulty override enabled but no CS/AR/OD override specified.");

                    if (!float.IsNaN(settings.SectionCircleSize) && (settings.SectionCircleSize < 0 || settings.SectionCircleSize > 11))
                        throw new InvalidOperationException($"Section {section.Id}: SectionCircleSize must be in [0, 11].");

                    if (!float.IsNaN(settings.SectionApproachRate) && settings.SectionApproachRate > 11)
                        throw new InvalidOperationException($"Section {section.Id}: SectionApproachRate must be <= 11.");

                    if (!float.IsNaN(settings.SectionOverallDifficulty) && (settings.SectionOverallDifficulty < 0 || settings.SectionOverallDifficulty > 11))
                        throw new InvalidOperationException($"Section {section.Id}: SectionOverallDifficulty must be in [0, 11].");

                    if (settings.EnableGradualDifficultyChange)
                    {
                        if (float.IsNaN(settings.GradualDifficultyChangeEndTimeMs))
                            throw new InvalidOperationException($"Section {section.Id}: GradualDifficultyChangeEndTimeMs is required when gradual change is enabled.");

                        if (settings.GradualDifficultyChangeEndTimeMs < section.StartTime)
                            throw new InvalidOperationException($"Section {section.Id}: GradualDifficultyChangeEndTimeMs must be >= section start.");

                        if (section.EndTime >= 0 && settings.GradualDifficultyChangeEndTimeMs > section.EndTime)
                            throw new InvalidOperationException($"Section {section.Id}: GradualDifficultyChangeEndTimeMs must be within section range.");
                    }
                }

                if (settings.EnableGradualDifficultyChange && !settings.EnableDifficultyOverrides)
                    throw new InvalidOperationException($"Section {section.Id}: gradual difficulty change requires difficulty overrides.");

                if (settings.KeepDifficultyOverridesAfterSection && !settings.EnableDifficultyOverrides)
                    throw new InvalidOperationException($"Section {section.Id}: keep difficulty overrides requires difficulty overrides.");

                if (i > 0)
                {
                    var prev = ordered[i - 1];
                    double prevEnd = prev.EndTime < 0 ? double.MaxValue : prev.EndTime;
                    if (section.StartTime < prevEnd)
                        throw new InvalidOperationException($"Sections {prev.Id} and {section.Id} overlap.");
                }
            }
        }

        private static void validateRange(float value, int id, string key)
        {
            if (float.IsNaN(value))
                return;

            if (value < -2 || value > 2)
                throw new InvalidOperationException($"Section {id}: {key} out of allowed range.");
        }

        private static void requireNotNaN(float value, int id, string key)
        {
            if (float.IsNaN(value))
                throw new InvalidOperationException($"Section {id}: {key} is required.");
        }
    }
}
