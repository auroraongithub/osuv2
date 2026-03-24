// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Beatmaps.SectionGimmicks
{
    public class SectionGimmickSettings
    {
        public bool EnableHPGimmick { get; set; }
        public bool EnableNoMiss { get; set; }
        public bool EnableCountLimits { get; set; }
        public bool EnableNoMissedSliderEnd { get; set; }
        public bool EnableGreatOffsetPenalty { get; set; }

        public int Max300s { get; set; } = -1;
        public int Max100s { get; set; } = -1;
        public int Max50s { get; set; } = -1;
        public int MaxMisses { get; set; } = -1;

        public float HP300 { get; set; } = float.NaN;
        public float HP100 { get; set; } = float.NaN;
        public float HP50 { get; set; } = float.NaN;
        public float HPMiss { get; set; } = float.NaN;

        public bool NoDrain { get; set; }
        public bool ReverseHP { get; set; }

        public float GreatOffsetThresholdMs { get; set; } = -1;
        public float GreatOffsetPenaltyHP { get; set; } = float.NaN;

        public bool EnableDifficultyOverrides { get; set; }
        public bool EnableGradualDifficultyChange { get; set; }
        public float GradualDifficultyChangeEndTimeMs { get; set; } = float.NaN;
        public bool KeepDifficultyOverridesAfterSection { get; set; }
        public float SectionCircleSize { get; set; } = float.NaN;
        public float SectionApproachRate { get; set; } = float.NaN;
        public float SectionOverallDifficulty { get; set; } = float.NaN;

        public bool ForceHidden { get; set; }
        public bool ForceNoApproachCircle { get; set; }
        public bool ForceHardRock { get; set; }
        public bool ForceFlashlight { get; set; }
        public bool ForceDoubleTime { get; set; }
        public bool ForceSingleTap { get; set; }
        public bool ForceAlternate { get; set; }

        public string SectionName { get; set; } = string.Empty;
        public Color4 DisplayColor { get; set; } = Color4.White;
    }
}
