// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.HitObjectGimmicks;
using osu.Game.Beatmaps.SectionGimmicks;
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

        public bool IsSelectionEnableHPGimmick
            => getSelectionBoolState(s => s.EnableHPGimmick);

        public bool IsSelectionEnableNoMiss
            => getSelectionBoolState(s => s.EnableNoMiss);

        public bool IsSelectionEnableCountLimits
            => getSelectionBoolState(s => s.EnableCountLimits);

        public bool IsSelectionEnableGreatOffsetPenalty
            => getSelectionBoolState(s => s.EnableGreatOffsetPenalty);

        public bool IsSelectionEnableDifficultyOverrides
            => getSelectionBoolState(s => s.EnableDifficultyOverrides);

        public bool IsSelectionForceHidden
            => getSelectionBoolState(s => s.ForceHidden);

        public bool IsSelectionForceHardRock
            => getSelectionBoolState(s => s.ForceHardRock);

        public bool IsSelectionForceFlashlight
            => getSelectionBoolState(s => s.ForceFlashlight);

        public SectionGimmickSettings? GetSelectionRepresentativeSettings()
        {
            var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();
            if (selected.Count == 0)
                return null;

            var lookup = createLookup(editorBeatmap.HitObjectGimmicks);
            var first = selected[0];

            if (!lookup.TryGetValue((first.StartTime, first.ComboIndexWithOffsets), out var firstSettings))
                return new SectionGimmickSettings();

            return mapToSectionSettings(firstSettings);
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

        public void SetSelectionBoolSetting(System.Action<HitObjectGimmickSettings, bool> setter, bool enabled)
        {
            var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();

            if (selected.Count == 0)
                return;

            var updated = CloneHitObjectGimmicks(editorBeatmap.HitObjectGimmicks ?? new BeatmapHitObjectGimmicks());

            foreach (var hitObject in selected)
            {
                var entry = getOrCreateEntry(updated, hitObject);
                setter(entry.Settings, enabled);
                cleanupEntryIfEmpty(updated, entry);
            }

            editorBeatmap.BeginChange();
            editorBeatmap.HitObjectGimmicks = updated;
            editorBeatmap.UpdateAllHitObjects();
            editorBeatmap.EndChange();
        }

        public void SetSelectionFloatSetting(System.Action<HitObjectGimmickSettings, float> setter, float value)
        {
            var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();

            if (selected.Count == 0)
                return;

            var updated = CloneHitObjectGimmicks(editorBeatmap.HitObjectGimmicks ?? new BeatmapHitObjectGimmicks());

            foreach (var hitObject in selected)
            {
                var entry = getOrCreateEntry(updated, hitObject);
                setter(entry.Settings, value);
                cleanupEntryIfEmpty(updated, entry);
            }

            editorBeatmap.BeginChange();
            editorBeatmap.HitObjectGimmicks = updated;
            editorBeatmap.UpdateAllHitObjects();
            editorBeatmap.EndChange();
        }

        public void SetSelectionIntSetting(System.Action<HitObjectGimmickSettings, int> setter, int value)
        {
            var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();

            if (selected.Count == 0)
                return;

            var updated = CloneHitObjectGimmicks(editorBeatmap.HitObjectGimmicks ?? new BeatmapHitObjectGimmicks());

            foreach (var hitObject in selected)
            {
                var entry = getOrCreateEntry(updated, hitObject);
                setter(entry.Settings, value);
                cleanupEntryIfEmpty(updated, entry);
            }

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
                            EnableHPGimmick = settings.EnableHPGimmick,
                            EnableNoMiss = settings.EnableNoMiss,
                            EnableCountLimits = settings.EnableCountLimits,
                            EnableGreatOffsetPenalty = settings.EnableGreatOffsetPenalty,
                            Max300s = settings.Max300s,
                            Max100s = settings.Max100s,
                            Max50s = settings.Max50s,
                            MaxMisses = settings.MaxMisses,
                            HP300 = settings.HP300,
                            HP100 = settings.HP100,
                            HP50 = settings.HP50,
                            HPMiss = settings.HPMiss,
                            GreatOffsetThresholdMs = settings.GreatOffsetThresholdMs,
                            GreatOffsetPenaltyHP = settings.GreatOffsetPenaltyHP,
                            EnableDifficultyOverrides = settings.EnableDifficultyOverrides,
                            SectionCircleSize = settings.SectionCircleSize,
                            SectionApproachRate = settings.SectionApproachRate,
                            SectionOverallDifficulty = settings.SectionOverallDifficulty,
                            ForceHidden = settings.ForceHidden,
                            ForceNoApproachCircle = settings.ForceNoApproachCircle,
                            ForceHardRock = settings.ForceHardRock,
                            ForceFlashlight = settings.ForceFlashlight,
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
                {
                    existing.Settings.ForceNoApproachCircle = false;
                    cleanupEntryIfEmpty(gimmicks, existing);
                }

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
            cleanupEntryIfEmpty(gimmicks, existing);
        }

        private bool getSelectionBoolState(System.Func<HitObjectGimmickSettings, bool> getter)
        {
            var selected = editorBeatmap.SelectedHitObjects.OfType<OsuHitObject>().ToList();

            if (selected.Count == 0)
                return false;

            var lookup = createLookup(editorBeatmap.HitObjectGimmicks);
            return selected.All(h =>
            {
                if (!lookup.TryGetValue((h.StartTime, h.ComboIndexWithOffsets), out var settings))
                    return false;

                return getter(settings);
            });
        }

        private static HitObjectGimmickEntry getOrCreateEntry(BeatmapHitObjectGimmicks gimmicks, OsuHitObject hitObject)
        {
            var existing = gimmicks.Entries.FirstOrDefault(e =>
                e.StartTime == hitObject.StartTime
                && e.ComboIndexWithOffsets == hitObject.ComboIndexWithOffsets);

            if (existing != null)
                return existing;

            existing = new HitObjectGimmickEntry
            {
                StartTime = hitObject.StartTime,
                ComboIndexWithOffsets = hitObject.ComboIndexWithOffsets,
                Settings = new HitObjectGimmickSettings(),
            };

            gimmicks.Entries.Add(existing);
            return existing;
        }

        private static void cleanupEntryIfEmpty(BeatmapHitObjectGimmicks gimmicks, HitObjectGimmickEntry entry)
        {
            var s = entry.Settings;

            bool hasAny = s.EnableHPGimmick
                          || s.EnableNoMiss
                          || s.EnableCountLimits
                          || s.EnableGreatOffsetPenalty
                          || s.EnableDifficultyOverrides
                          || s.ForceHidden
                          || s.ForceNoApproachCircle
                          || s.ForceHardRock
                          || s.ForceFlashlight
                          || s.Max300s >= 0
                          || s.Max100s >= 0
                          || s.Max50s >= 0
                          || s.MaxMisses >= 0
                          || !float.IsNaN(s.HP300)
                          || !float.IsNaN(s.HP100)
                          || !float.IsNaN(s.HP50)
                          || !float.IsNaN(s.HPMiss)
                          || s.GreatOffsetThresholdMs >= 0
                          || !float.IsNaN(s.GreatOffsetPenaltyHP)
                          || !float.IsNaN(s.SectionCircleSize)
                          || !float.IsNaN(s.SectionApproachRate)
                          || !float.IsNaN(s.SectionOverallDifficulty);

            if (!hasAny)
                gimmicks.Entries.Remove(entry);
        }

        private static SectionGimmickSettings mapToSectionSettings(HitObjectGimmickSettings source)
            => new SectionGimmickSettings
            {
                EnableHPGimmick = source.EnableHPGimmick,
                EnableNoMiss = source.EnableNoMiss,
                EnableCountLimits = source.EnableCountLimits,
                EnableGreatOffsetPenalty = source.EnableGreatOffsetPenalty,

                Max300s = source.Max300s,
                Max100s = source.Max100s,
                Max50s = source.Max50s,
                MaxMisses = source.MaxMisses,

                HP300 = source.HP300,
                HP100 = source.HP100,
                HP50 = source.HP50,
                HPMiss = source.HPMiss,

                GreatOffsetThresholdMs = source.GreatOffsetThresholdMs,
                GreatOffsetPenaltyHP = source.GreatOffsetPenaltyHP,

                EnableDifficultyOverrides = source.EnableDifficultyOverrides,
                SectionCircleSize = source.SectionCircleSize,
                SectionApproachRate = source.SectionApproachRate,
                SectionOverallDifficulty = source.SectionOverallDifficulty,

                ForceHidden = source.ForceHidden,
                ForceNoApproachCircle = source.ForceNoApproachCircle,
                ForceHardRock = source.ForceHardRock,
                ForceFlashlight = source.ForceFlashlight,
            };

        private static Dictionary<(double StartTime, int ComboIndexWithOffsets), HitObjectGimmickSettings> createLookup(BeatmapHitObjectGimmicks gimmicks)
            => gimmicks.Entries.ToDictionary(e => (e.StartTime, e.ComboIndexWithOffsets), e => e.Settings ?? new HitObjectGimmickSettings());

        private static bool isNoApproachForced(OsuHitObject hitObject, Dictionary<(double StartTime, int ComboIndexWithOffsets), HitObjectGimmickSettings> lookup)
            => lookup.TryGetValue((hitObject.StartTime, hitObject.ComboIndexWithOffsets), out HitObjectGimmickSettings? settings) && settings.ForceNoApproachCircle;
    }
}
