// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Tests.Mods;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class SectionGimmickInputBlockingTest : OsuModTestScene
    {
        [Test]
        public void TestSectionForcedSingleTapBlocksAlternatingInput() => CreateModTest(new ModTestData
        {
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1,
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                SectionGimmicks = new BeatmapSectionGimmicks
                {
                    Sections =
                    {
                        new SectionGimmickSection
                        {
                            Id = 0,
                            StartTime = 0,
                            EndTime = -1,
                            Settings = new SectionGimmickSettings
                            {
                                ForceSingleTap = true,
                            }
                        }
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(200, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.RightButton),
                new OsuReplayFrame(1001, new Vector2(200, 100)),
            }
        });

        [Test]
        public void TestSectionForcedAlternateBlocksSameKeyInput() => CreateModTest(new ModTestData
        {
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1,
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                SectionGimmicks = new BeatmapSectionGimmicks
                {
                    Sections =
                    {
                        new SectionGimmickSection
                        {
                            Id = 0,
                            StartTime = 0,
                            EndTime = -1,
                            Settings = new SectionGimmickSettings
                            {
                                ForceAlternate = true,
                            }
                        }
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(200, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.LeftButton),
                new OsuReplayFrame(1001, new Vector2(200, 100)),
            }
        });
    }
}
