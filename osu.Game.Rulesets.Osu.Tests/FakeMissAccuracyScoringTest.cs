// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class FakeMissAccuracyScoringTest
    {
        [Test]
        public void TestFakeMissPunishmentAffectsAccuracyLikeRegularMiss()
        {
            var beatmap = new Beatmap<OsuHitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 },
                    new FakeHitCircle { StartTime = 2000 },
                }
            };

            var scoreProcessor = new OsuScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            scoreProcessor.ApplyResult(new OsuJudgementResult(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement()) { Type = HitResult.Great });
            scoreProcessor.ApplyResult(new OsuJudgementResult(beatmap.HitObjects[1], beatmap.HitObjects[1].CreateJudgement()) { Type = HitResult.Miss });

            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(0.5).Within(0.0001));
        }
    }
}
