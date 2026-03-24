// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.HitObjectGimmicks;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class HitObjectGimmickEditorModelTest
    {
        [Test]
        public void TestSetSelectionForceNoApproachCircleAddsAndRemovesEntries()
        {
            var beatmap = new osu.Game.Rulesets.Osu.Beatmaps.OsuBeatmap();
            var first = new HitCircle { StartTime = 500 };
            var second = new HitCircle { StartTime = 1000, NewCombo = true, ComboOffset = 1 };

            beatmap.HitObjects.Add(first);
            beatmap.HitObjects.Add(second);

            beatmap.BeatmapInfo.Ruleset = new osu.Game.Rulesets.Osu.OsuRuleset().RulesetInfo;
            var editorBeatmap = new EditorBeatmap(beatmap, beatmapInfo: beatmap.BeatmapInfo);

            first.UpdateComboInformation(null);
            second.UpdateComboInformation(first);

            editorBeatmap.SelectedHitObjects.Add(first);

            var model = new HitObjectGimmickEditorModel(editorBeatmap);

            model.SetSelectionForceNoApproachCircle(true);

            Assert.That(editorBeatmap.HitObjectGimmicks.Entries.Count, Is.EqualTo(1));
            var entry = editorBeatmap.HitObjectGimmicks.Entries[0];
            Assert.That(entry.StartTime, Is.EqualTo(first.StartTime));
            Assert.That(entry.ComboIndexWithOffsets, Is.EqualTo(first.ComboIndexWithOffsets));
            Assert.That(entry.Settings.ForceNoApproachCircle, Is.True);

            model.SetSelectionForceNoApproachCircle(false);

            Assert.That(editorBeatmap.HitObjectGimmicks.Entries.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestSelectionStateReflectsAppliedEntries()
        {
            var beatmap = new osu.Game.Rulesets.Osu.Beatmaps.OsuBeatmap();
            var first = new HitCircle { StartTime = 500 };
            var second = new HitCircle { StartTime = 500, NewCombo = true, ComboOffset = 1 };

            beatmap.HitObjects.Add(first);
            beatmap.HitObjects.Add(second);

            beatmap.BeatmapInfo.Ruleset = new osu.Game.Rulesets.Osu.OsuRuleset().RulesetInfo;
            var editorBeatmap = new EditorBeatmap(beatmap, beatmapInfo: beatmap.BeatmapInfo);

            first.UpdateComboInformation(null);
            second.UpdateComboInformation(first);

            editorBeatmap.HitObjectGimmicks = new BeatmapHitObjectGimmicks
            {
                Entries =
                {
                    new HitObjectGimmickEntry
                    {
                        StartTime = first.StartTime,
                        ComboIndexWithOffsets = first.ComboIndexWithOffsets,
                        Settings = new HitObjectGimmickSettings
                        {
                            ForceNoApproachCircle = true,
                        }
                    }
                }
            };

            editorBeatmap.SelectedHitObjects.Add(first);

            var model = new HitObjectGimmickEditorModel(editorBeatmap);

            Assert.That(model.HasSelection, Is.True);
            Assert.That(model.IsSelectionNoApproachCircleForced, Is.True);

            editorBeatmap.SelectedHitObjects.Add(second);
            Assert.That(model.IsSelectionNoApproachCircleForced, Is.False);
        }
    }
}
