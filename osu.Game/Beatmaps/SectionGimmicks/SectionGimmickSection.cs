// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps.SectionGimmicks
{
    public class SectionGimmickSection
    {
        public int Id { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public SectionGimmickSettings Settings { get; set; } = new SectionGimmickSettings();

        public bool Contains(double time)
            => time >= StartTime && (EndTime < 0 || time <= EndTime);
    }
}
