// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps.HitObjectGimmicks
{
    public class HitObjectGimmickEntry
    {
        public double StartTime { get; set; }

        public int ComboIndexWithOffsets { get; set; }

        public HitObjectGimmickSettings Settings { get; set; } = new HitObjectGimmickSettings();
    }
}
