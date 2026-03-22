// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.SectionGimmicks;

namespace osu.Game.Screens.Edit.Compose
{
    public class SectionGimmickEditorModel
    {
        private readonly EditorBeatmap editorBeatmap;

        private SectionGimmickSettings? copiedSettings;

        public readonly BindableList<SectionGimmickSection> Sections = new BindableList<SectionGimmickSection>();

        public readonly BindableInt SelectedSectionId = new BindableInt(-1);

        public bool HasCopiedSettings => copiedSettings != null;

        public SectionGimmickEditorModel(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;

            if (this.editorBeatmap.SectionGimmicks == null)
                this.editorBeatmap.SectionGimmicks = new BeatmapSectionGimmicks();

            syncFromBeatmap();
        }

        public void AddSection(double time)
        {
            mutate(sections =>
            {
                int newId = sections.Count > 0 ? sections.Max(s => s.Id) + 1 : 0;

                double startTime = time;

                if (sections.Count > 0)
                {
                    var latest = sections.MaxBy(s => s.StartTime)!;

                    if (latest.EndTime < 0)
                    {
                        startTime = Math.Max(startTime, latest.StartTime + 1);
                        latest.EndTime = startTime;
                    }
                    else if (startTime < latest.EndTime)
                        startTime = latest.EndTime;
                }

                var newSettings = new SectionGimmickSettings();

                // If previous section has "Keep overrides after section" enabled,
                // inherit difficulty override values from the previous section
                if (sections.Count > 0)
                {
                    var latest = sections.MaxBy(s => s.StartTime)!;
                    if (latest.Settings.KeepDifficultyOverridesAfterSection &&
                        latest.Settings.EnableDifficultyOverrides)
                    {
                        newSettings.EnableDifficultyOverrides = true;
                        newSettings.SectionCircleSize = latest.Settings.SectionCircleSize;
                        newSettings.SectionApproachRate = latest.Settings.SectionApproachRate;
                        newSettings.SectionOverallDifficulty = latest.Settings.SectionOverallDifficulty;
                    }
                }

                sections.Add(new SectionGimmickSection
                {
                    Id = newId,
                    StartTime = startTime,
                    EndTime = -1,
                    Settings = newSettings,
                });

                return newId;
            });
        }

        public void RemoveSelectedSection()
        {
            int selectedId = SelectedSectionId.Value;

            if (selectedId < 0)
                return;

            mutate(sections =>
            {
                sections.RemoveAll(s => s.Id == selectedId);
                return sections.FirstOrDefault()?.Id ?? -1;
            });
        }

        public void SetSelectedStartTime(double startTime)
        {
            mutateSelectedSection(section => section.StartTime = startTime);
        }

        public void SetSelectedEndTime(double endTime)
        {
            mutateSelectedSection(section => section.EndTime = endTime);
        }

        public void SetSelectedSetting(Action<SectionGimmickSettings> settingMutation)
        {
            mutateSelectedSection(section => settingMutation(section.Settings));
        }

        public void CopySelectedSettings()
        {
            var selected = Sections.FirstOrDefault(s => s.Id == SelectedSectionId.Value);

            if (selected == null)
                return;

            copiedSettings = cloneSettings(selected.Settings);
        }

        public void PasteSettingsTo(IEnumerable<int> targetSectionIds)
        {
            if (copiedSettings == null)
                return;

            int[] targets = targetSectionIds.Distinct().ToArray();

            if (targets.Length == 0 && SelectedSectionId.Value >= 0)
                targets = [SelectedSectionId.Value];

            if (targets.Length == 0)
                return;

            mutate(sections =>
            {
                foreach (int id in targets)
                {
                    int index = sections.FindIndex(s => s.Id == id);

                    if (index < 0)
                        continue;

                    sections[index].Settings = cloneSettings(copiedSettings);
                }

                return targets[0];
            });
        }

        public BeatmapSectionGimmicks CreateClonedCurrentGimmicks()
            => cloneGimmicks(editorBeatmap.SectionGimmicks ?? new BeatmapSectionGimmicks());

        public static BeatmapSectionGimmicks CloneGimmicks(BeatmapSectionGimmicks source)
            => cloneGimmicks(source);

        private void mutateSelectedSection(Action<SectionGimmickSection> sectionMutation)
        {
            int selectedId = SelectedSectionId.Value;

            if (selectedId < 0)
                return;

            mutate(sections =>
            {
                var selected = sections.FirstOrDefault(s => s.Id == selectedId);

                if (selected == null)
                    return selectedId;

                sectionMutation(selected);
                return selectedId;
            });
        }

        private void mutate(Func<List<SectionGimmickSection>, int?> mutation)
        {
            editorBeatmap.BeginChange();

            try
            {
                var sections = cloneSections((editorBeatmap.SectionGimmicks ?? new BeatmapSectionGimmicks()).Sections);
                int? preferredSelection = mutation(sections);

                editorBeatmap.SectionGimmicks = new BeatmapSectionGimmicks
                {
                    Sections = sections.OrderBy(s => s.StartTime).ToList(),
                };

                syncFromBeatmap(preferredSelection);
            }
            finally
            {
                editorBeatmap.EndChange();
            }
        }

        private void syncFromBeatmap(int? preferredSelection = null)
        {
            int selectionId = preferredSelection ?? SelectedSectionId.Value;

            Sections.Clear();
            var source = editorBeatmap.SectionGimmicks ?? new BeatmapSectionGimmicks();
            Sections.AddRange(cloneSections(source.Sections.OrderBy(s => s.StartTime)));

            if (Sections.All(s => s.Id != selectionId))
                selectionId = Sections.FirstOrDefault()?.Id ?? -1;

            SelectedSectionId.Value = selectionId;
        }

        private static BeatmapSectionGimmicks cloneGimmicks(BeatmapSectionGimmicks source)
            => new BeatmapSectionGimmicks
            {
                Sections = cloneSections(source.Sections),
            };

        private static List<SectionGimmickSection> cloneSections(IEnumerable<SectionGimmickSection>? sections)
            => sections?.Select(cloneSection).ToList() ?? new List<SectionGimmickSection>();

        private static SectionGimmickSection cloneSection(SectionGimmickSection section)
            => new SectionGimmickSection
            {
                Id = section.Id,
                StartTime = section.StartTime,
                EndTime = section.EndTime,
                Settings = cloneSettings(section.Settings ?? new SectionGimmickSettings()),
            };

        private static SectionGimmickSettings cloneSettings(SectionGimmickSettings settings)
            => new SectionGimmickSettings
            {
                EnableHPGimmick = settings.EnableHPGimmick,
                EnableNoMiss = settings.EnableNoMiss,
                EnableCountLimits = settings.EnableCountLimits,
                EnableNoMissedSliderEnd = settings.EnableNoMissedSliderEnd,
                EnableGreatOffsetPenalty = settings.EnableGreatOffsetPenalty,
                Max300s = settings.Max300s,
                Max100s = settings.Max100s,
                Max50s = settings.Max50s,
                HP300 = settings.HP300,
                HP100 = settings.HP100,
                HP50 = settings.HP50,
                HPMiss = settings.HPMiss,
                NoDrain = settings.NoDrain,
                ReverseHP = settings.ReverseHP,
                GreatOffsetThresholdMs = settings.GreatOffsetThresholdMs,
                GreatOffsetPenaltyHP = settings.GreatOffsetPenaltyHP,
                EnableDifficultyOverrides = settings.EnableDifficultyOverrides,
                EnableGradualDifficultyChange = settings.EnableGradualDifficultyChange,
                GradualDifficultyChangeEndTimeMs = settings.GradualDifficultyChangeEndTimeMs,
                KeepDifficultyOverridesAfterSection = settings.KeepDifficultyOverridesAfterSection,
                SectionCircleSize = settings.SectionCircleSize,
                SectionApproachRate = settings.SectionApproachRate,
                SectionOverallDifficulty = settings.SectionOverallDifficulty,
                ForceHidden = settings.ForceHidden,
                ForceHardRock = settings.ForceHardRock,
                ForceFlashlight = settings.ForceFlashlight,
                ForceDoubleTime = settings.ForceDoubleTime,
                SectionName = settings.SectionName,
                DisplayColor = settings.DisplayColor,
            };
    }
}
