// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.HitObjectGimmicks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class HitObjectGimmickEditorModel
    {
        private readonly EditorBeatmap editorBeatmap;

        public HitObjectGimmickEditorModel(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;
        }

        public bool HasSelection => editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().Any();

        public bool IsSelectionNoApproachCircleForced
        {
            get
            {
                var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();

                if (selected.Count == 0)
                    return false;

                var lookup = createLookup(editorBeatmap.HitObjectGimmicks);
                return selected.All(h => isNoApproachForced(h, lookup));
            }
        }

        public void SetSelectionForceNoApproachCircle(bool enabled)
        {
            var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();

            if (selected.Count == 0)
                return;

            var updated = CloneHitObjectGimmicks(editorBeatmap.HitObjectGimmicks ?? new BeatmapHitObjectGimmicks());

            foreach (var hitObject in selected)
                applyOrRemoveEntry(updated, hitObject, enabled);

            editorBeatmap.BeginChange();
            editorBeatmap.HitObjectGimmicks = updated;
            editorBeatmap.UpdateAllHitObjects();
            editorBeatmap.EndChange();
        }

        public static BeatmapHitObjectGimmicks CloneHitObjectGimmicks(BeatmapHitObjectGimmicks source)
            => new BeatmapHitObjectGimmicks
            {
                Entries = source.Entries.Select(e =>
                {
                    var settings = e.Settings ?? new HitObjectGimmickSettings();

                    return new HitObjectGimmickEntry
                    {
                        StartTime = e.StartTime,
                        ComboIndexWithOffsets = e.ComboIndexWithOffsets,
                        Settings = new HitObjectGimmickSettings
                        {
                            ForceNoApproachCircle = settings.ForceNoApproachCircle,
                        }
                    };
                }).ToList(),
            };

        private static void applyOrRemoveEntry(BeatmapHitObjectGimmicks gimmicks, OsuHitObject hitObject, bool enabled)
        {
            var existing = gimmicks.Entries.FirstOrDefault(e =>
                e.StartTime == hitObject.StartTime
                && e.ComboIndexWithOffsets == hitObject.ComboIndexWithOffsets);

            if (!enabled)
            {
                if (existing != null)
                    gimmicks.Entries.Remove(existing);

                return;
            }

            if (existing == null)
            {
                existing = new HitObjectGimmickEntry
                {
                    StartTime = hitObject.StartTime,
                    ComboIndexWithOffsets = hitObject.ComboIndexWithOffsets,
                    Settings = new HitObjectGimmickSettings(),
                };

                gimmicks.Entries.Add(existing);
            }

            existing.Settings.ForceNoApproachCircle = true;
        }

        private static Dictionary<(double StartTime, int ComboIndexWithOffsets), bool> createLookup(BeatmapHitObjectGimmicks gimmicks)
            => gimmicks.Entries.ToDictionary(e => (e.StartTime, e.ComboIndexWithOffsets), e => e.Settings?.ForceNoApproachCircle == true);

        private static bool isNoApproachForced(OsuHitObject hitObject, Dictionary<(double StartTime, int ComboIndexWithOffsets), bool> lookup)
            => lookup.TryGetValue((hitObject.StartTime, hitObject.ComboIndexWithOffsets), out bool enabled) && enabled;
    }
}
