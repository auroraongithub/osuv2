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

        [Test]
        public void TestSectionDifficultyOverrideGradualArChange()
        {
            var early = new HitCircle { StartTime = 200 };
            var mid = new HitCircle { StartTime = 700 };
            var late = new HitCircle { StartTime = 1300 };

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(early);
            beatmap.HitObjects.Add(mid);
            beatmap.HitObjects.Add(late);

            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 0,
                StartTime = 0,
                EndTime = 1500,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    EnableGradualDifficultyChange = true,
                    GradualDifficultyChangeEndTimeMs = 1000,
                    SectionApproachRate = 10,
                }
            });

            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            processor.PostProcess();

            Assert.That(early.TimePreempt, Is.GreaterThan(mid.TimePreempt));
            Assert.That(mid.TimePreempt, Is.GreaterThan(late.TimePreempt));

            // after gradual finish point, should be at target AR10 preempt.
            Assert.That(late.TimePreempt, Is.EqualTo(450).Within(0.0001));
        }

        [Test]
        public void TestSectionDifficultyOverrideKeepAfterSection()
        {
            var inSection = new HitCircle { StartTime = 500 };
            var afterSection = new HitCircle { StartTime = 2000 };

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(inSection);
            beatmap.HitObjects.Add(afterSection);

            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 0,
                StartTime = 0,
                EndTime = 1000,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    KeepDifficultyOverridesAfterSection = true,
                    SectionApproachRate = 10,
                }
            });

            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            processor.PostProcess();

            Assert.That(inSection.TimePreempt, Is.EqualTo(450).Within(0.0001));
            Assert.That(afterSection.TimePreempt, Is.EqualTo(450).Within(0.0001));
        }

        [Test]
        public void TestSectionInheritsDifficultyFromPreviousSection()
        {
            var section0Object = new HitCircle { StartTime = 500 };
            var section1EarlyObject = new HitCircle { StartTime = 1500 };
            var section1LateObject = new HitCircle { StartTime = 2500 };

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(section0Object);
            beatmap.HitObjects.Add(section1EarlyObject);
            beatmap.HitObjects.Add(section1LateObject);

            // Set a specific base difficulty to make the test deterministic
            beatmap.Difficulty.ApproachRate = 5; // AR5 preempt = 1200ms

            // Section 0: AR=8, keeps overrides after section
            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 0,
                StartTime = 0,
                EndTime = 1000,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    KeepDifficultyOverridesAfterSection = true,
                    SectionApproachRate = 8,
                }
            });

            // Section 1: Gradual AR shift to AR=10, should start from AR=8 (inherited)
            beatmap.SectionGimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 1,
                StartTime = 1000,
                EndTime = 3000,
                Settings = new SectionGimmickSettings
                {
                    EnableDifficultyOverrides = true,
                    EnableGradualDifficultyChange = true,
                    GradualDifficultyChangeEndTimeMs = 2000,
                    SectionApproachRate = 10,
                }
            });

            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            processor.PostProcess();

            // AR8 preempt is 600ms, AR10 preempt is 450ms
            var ar8Preempt = 600;
            var ar10Preempt = 450;

            // Section 0 object should have AR=8
            Assert.That(section0Object.TimePreempt, Is.EqualTo(ar8Preempt).Within(0.0001));

            // Section 1 early object should be halfway through gradual shift from AR8 to AR10
            // At 1500ms (progress 0.5), should be AR9 = (600 + 450) / 2 = 525ms
            var ar9Preempt = 525;
            Assert.That(section1EarlyObject.TimePreempt, Is.EqualTo(ar9Preempt).Within(0.0001));

            // Section 1 late object should have AR=10 (after gradual shift completes)
            Assert.That(section1LateObject.TimePreempt, Is.EqualTo(ar10Preempt).Within(0.0001));
        }
    }
}
