// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class SectionGimmickDifficultyOverrideTest
    {
        [Test]
        public void TestSectionDifficultyOverrideAppliesARAndOD()
        {
            var first = new HitCircle { StartTime = 1000 };
            var second = new HitCircle { StartTime = 3000 };

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(first);
            beatmap.HitObjects.Add(second);

            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 0,
                StartTime = 0,
                EndTime = 2000,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    SectionApproachRate = 10,
                    SectionOverallDifficulty = 9,
                }
            });

            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            processor.PostProcess();

            // AR10 preempt in osu! is 450ms.
            Assert.That(first.TimePreempt, Is.EqualTo(450).Within(0.0001));
            Assert.That(second.TimePreempt, Is.EqualTo(1200).Within(0.0001));

            var od9Windows = new OsuHitWindows();
            od9Windows.SetDifficulty(9);

            var baseOdWindows = new OsuHitWindows();
            baseOdWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            Assert.That(first.HitWindows.WindowFor(HitResult.Great), Is.EqualTo(od9Windows.WindowFor(HitResult.Great)).Within(0.0001));
            Assert.That(second.HitWindows.WindowFor(HitResult.Great), Is.EqualTo(baseOdWindows.WindowFor(HitResult.Great)).Within(0.0001));
        }

        [Test]
        public void TestSectionDifficultyOverrideAppliesCircleSize()
        {
            var first = new HitCircle { StartTime = 1000 };
            var second = new HitCircle { StartTime = 3000 };

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(first);
            beatmap.HitObjects.Add(second);

            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 0,
                StartTime = 0,
                EndTime = 2000,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    SectionCircleSize = 7,
                }
            });

            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            processor.PostProcess();

            Assert.That(first.Scale, Is.Not.EqualTo(second.Scale));
            Assert.That(first.Scale, Is.LessThan(second.Scale));
        }

        [Test]
        public void TestSectionDifficultyOverrideAllowsAr11AndOd11()
        {
            var hit = new HitCircle { StartTime = 1000 };

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(hit);

            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 0,
                StartTime = 0,
                EndTime = 2000,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    SectionApproachRate = 11,
                    SectionOverallDifficulty = 11,
                }
            });

            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            processor.PostProcess();

            // AR11 should be below AR10 preempt 450ms.
            Assert.That(hit.TimePreempt, Is.LessThan(450));

            // OD11 should make great window stricter than OD10.
            var od10 = new OsuHitWindows();
            od10.SetDifficulty(10);
            Assert.That(hit.HitWindows.WindowFor(HitResult.Great), Is.LessThan(od10.WindowFor(HitResult.Great)));
        }
    }
}
