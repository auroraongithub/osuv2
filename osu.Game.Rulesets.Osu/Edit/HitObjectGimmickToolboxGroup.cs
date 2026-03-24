// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class HitObjectGimmickToolboxGroup : EditorToolboxGroup
    {
        [Resolved]
        private osu.Game.Screens.Edit.EditorBeatmap editorBeatmap { get; set; } = null!;

        private HitObjectGimmickEditorModel model = null!;

        private FormCheckBox enableHpGimmick = null!;
        private FormNumberBox hp300 = null!;
        private FormNumberBox hp100 = null!;
        private FormNumberBox hp50 = null!;
        private FormNumberBox hpMiss = null!;

        private FormCheckBox enableNoMiss = null!;

        private FormCheckBox enableCountLimits = null!;
        private FormNumberBox max300 = null!;
        private FormNumberBox max100 = null!;
        private FormNumberBox max50 = null!;
        private FormNumberBox maxMiss = null!;

        private FormCheckBox enableGreatOffsetPenalty = null!;
        private FormNumberBox greatOffsetThreshold = null!;
        private FormNumberBox greatOffsetPenaltyHp = null!;

        private FormCheckBox enableDifficultyOverrides = null!;
        private FormNumberBox sectionCircleSize = null!;
        private FormNumberBox sectionApproachRate = null!;
        private FormNumberBox sectionOverallDifficulty = null!;

        private FormCheckBox forceHidden = null!;
        private FormCheckBox forceHardRock = null!;
        private FormCheckBox forceFlashlight = null!;
        private FormCheckBox forceNoApproachCircle = null!;

        private FillFlowContainer hpFields = null!;
        private FillFlowContainer countLimitFields = null!;
        private FillFlowContainer offsetPenaltyFields = null!;
        private FillFlowContainer difficultyOverrideFields = null!;

        private OsuSpriteText selectionStatus = null!;

        private bool updatingControls;

        public HitObjectGimmickToolboxGroup()
            : base("hit object gimmicks")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            model = new HitObjectGimmickEditorModel(editorBeatmap);

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    selectionStatus = new OsuSpriteText
                    {
                        Text = "No object selected",
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                        Colour = Color4.Gray,
                    },

                    enableHpGimmick = new FormCheckBox { Caption = "HP Gimmick" },
                    hpFields = createContainer(
                        hp300 = new FormNumberBox(allowDecimals: true) { Caption = "HP300" },
                        hp100 = new FormNumberBox(allowDecimals: true) { Caption = "HP100" },
                        hp50 = new FormNumberBox(allowDecimals: true) { Caption = "HP50" },
                        hpMiss = new FormNumberBox(allowDecimals: true) { Caption = "HPMiss" }),

                    enableNoMiss = new FormCheckBox { Caption = "No Miss" },

                    enableCountLimits = new FormCheckBox { Caption = "Count Limits" },
                    countLimitFields = createContainer(
                        max300 = new FormNumberBox { Caption = "Max300s (-1 disabled)" },
                        max100 = new FormNumberBox { Caption = "Max100s (-1 disabled)" },
                        max50 = new FormNumberBox { Caption = "Max50s (-1 disabled)" },
                        maxMiss = new FormNumberBox { Caption = "MaxMisses (-1 disabled)" }),

                    enableGreatOffsetPenalty = new FormCheckBox { Caption = "Great Offset Penalty" },
                    offsetPenaltyFields = createContainer(
                        greatOffsetThreshold = new FormNumberBox(allowDecimals: true) { Caption = "GreatOffsetThresholdMs" },
                        greatOffsetPenaltyHp = new FormNumberBox(allowDecimals: true) { Caption = "GreatOffsetPenaltyHP" }),

                    enableDifficultyOverrides = new FormCheckBox { Caption = "Difficulty Overrides (CS/AR/OD)" },
                    difficultyOverrideFields = createContainer(
                        sectionCircleSize = new FormNumberBox(allowDecimals: true) { Caption = "SectionCircleSize (0-11)" },
                        sectionApproachRate = new FormNumberBox(allowDecimals: true) { Caption = "SectionApproachRate (<= 11)" },
                        sectionOverallDifficulty = new FormNumberBox(allowDecimals: true) { Caption = "SectionOverallDifficulty (0-11)" }),

                    forceHidden = new FormCheckBox { Caption = "Force Hidden (HD)" },
                    forceHardRock = new FormCheckBox { Caption = "Force Hard Rock (HR)" },
                    forceFlashlight = new FormCheckBox { Caption = "Force Flashlight (FL)" },
                    forceNoApproachCircle = new FormCheckBox { Caption = "Force No Approach Circle" },
                }
            };

            bindControlEvents();

            editorBeatmap.SelectedHitObjects.BindCollectionChanged((_, _) => updateFromSelection(), true);
            editorBeatmap.HitObjectUpdated += _ => updateFromSelection();
            editorBeatmap.HitObjectAdded += _ => updateFromSelection();
            editorBeatmap.HitObjectRemoved += _ => updateFromSelection();
            editorBeatmap.BeatmapReprocessed += updateFromSelection;
        }

        private void bindControlEvents()
        {
            enableHpGimmick.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableHPGimmick = value));
            enableNoMiss.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableNoMiss = value));
            enableCountLimits.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableCountLimits = value));
            enableGreatOffsetPenalty.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableGreatOffsetPenalty = value));
            enableDifficultyOverrides.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableDifficultyOverrides = value));

            forceHidden.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceHidden = value));
            forceHardRock.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceHardRock = value));
            forceFlashlight.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceFlashlight = value));
            forceNoApproachCircle.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceNoApproachCircle = value));

            hp300.OnCommit += (_, _) => setFloat(hp300, (s, value) => s.HP300 = value);
            hp100.OnCommit += (_, _) => setFloat(hp100, (s, value) => s.HP100 = value);
            hp50.OnCommit += (_, _) => setFloat(hp50, (s, value) => s.HP50 = value);
            hpMiss.OnCommit += (_, _) => setFloat(hpMiss, (s, value) => s.HPMiss = value);

            max300.OnCommit += (_, _) => setInt(max300, (s, value) => s.Max300s = value);
            max100.OnCommit += (_, _) => setInt(max100, (s, value) => s.Max100s = value);
            max50.OnCommit += (_, _) => setInt(max50, (s, value) => s.Max50s = value);
            maxMiss.OnCommit += (_, _) => setInt(maxMiss, (s, value) => s.MaxMisses = value);

            greatOffsetThreshold.OnCommit += (_, _) => setFloat(greatOffsetThreshold, (s, value) => s.GreatOffsetThresholdMs = value);
            greatOffsetPenaltyHp.OnCommit += (_, _) => setFloat(greatOffsetPenaltyHp, (s, value) => s.GreatOffsetPenaltyHP = value);

            sectionCircleSize.OnCommit += (_, _) => setFloat(sectionCircleSize, (s, value) => s.SectionCircleSize = value);
            sectionApproachRate.OnCommit += (_, _) => setFloat(sectionApproachRate, (s, value) => s.SectionApproachRate = value);
            sectionOverallDifficulty.OnCommit += (_, _) => setFloat(sectionOverallDifficulty, (s, value) => s.SectionOverallDifficulty = value);
        }

        private void updateFromSelection()
        {
            updatingControls = true;

            bool hasSelection = model.HasSelection;

            selectionStatus.Text = hasSelection
                ? $"Selected objects: {editorBeatmap.SelectedHitObjects.Count}"
                : "No object selected";
            selectionStatus.Colour = hasSelection ? Color4.White : Color4.Gray;

            bool enabled = hasSelection;
            setEnabledState(enabled,
                enableHpGimmick,
                hp300, hp100, hp50, hpMiss,
                enableNoMiss,
                enableCountLimits, max300, max100, max50, maxMiss,
                enableGreatOffsetPenalty, greatOffsetThreshold, greatOffsetPenaltyHp,
                enableDifficultyOverrides, sectionCircleSize, sectionApproachRate, sectionOverallDifficulty,
                forceHidden, forceHardRock, forceFlashlight, forceNoApproachCircle);

            enableHpGimmick.Current.Value = hasSelection && model.IsSelectionEnableHPGimmick;
            enableNoMiss.Current.Value = hasSelection && model.IsSelectionEnableNoMiss;
            enableCountLimits.Current.Value = hasSelection && model.IsSelectionEnableCountLimits;
            enableGreatOffsetPenalty.Current.Value = hasSelection && model.IsSelectionEnableGreatOffsetPenalty;
            enableDifficultyOverrides.Current.Value = hasSelection && model.IsSelectionEnableDifficultyOverrides;

            forceHidden.Current.Value = hasSelection && model.IsSelectionForceHidden;
            forceHardRock.Current.Value = hasSelection && model.IsSelectionForceHardRock;
            forceFlashlight.Current.Value = hasSelection && model.IsSelectionForceFlashlight;
            forceNoApproachCircle.Current.Value = hasSelection && model.IsSelectionNoApproachCircleForced;

            var representative = model.GetSelectionRepresentativeSettings();
            hp300.Current.Value = formatFloat(representative?.HP300 ?? float.NaN);
            hp100.Current.Value = formatFloat(representative?.HP100 ?? float.NaN);
            hp50.Current.Value = formatFloat(representative?.HP50 ?? float.NaN);
            hpMiss.Current.Value = formatFloat(representative?.HPMiss ?? float.NaN);

            max300.Current.Value = formatInt(representative?.Max300s ?? -1);
            max100.Current.Value = formatInt(representative?.Max100s ?? -1);
            max50.Current.Value = formatInt(representative?.Max50s ?? -1);
            maxMiss.Current.Value = formatInt(representative?.MaxMisses ?? -1);

            greatOffsetThreshold.Current.Value = formatFloat(representative?.GreatOffsetThresholdMs ?? -1);
            greatOffsetPenaltyHp.Current.Value = formatFloat(representative?.GreatOffsetPenaltyHP ?? float.NaN);

            sectionCircleSize.Current.Value = formatFloat(representative?.SectionCircleSize ?? float.NaN);
            sectionApproachRate.Current.Value = formatFloat(representative?.SectionApproachRate ?? float.NaN);
            sectionOverallDifficulty.Current.Value = formatFloat(representative?.SectionOverallDifficulty ?? float.NaN);

            hpFields.FadeTo(enableHpGimmick.Current.Value ? 1 : 0, 200);
            hpFields.AlwaysPresent = enableHpGimmick.Current.Value;

            countLimitFields.FadeTo(enableCountLimits.Current.Value ? 1 : 0, 200);
            countLimitFields.AlwaysPresent = enableCountLimits.Current.Value;

            offsetPenaltyFields.FadeTo(enableGreatOffsetPenalty.Current.Value ? 1 : 0, 200);
            offsetPenaltyFields.AlwaysPresent = enableGreatOffsetPenalty.Current.Value;

            difficultyOverrideFields.FadeTo(enableDifficultyOverrides.Current.Value ? 1 : 0, 200);
            difficultyOverrideFields.AlwaysPresent = enableDifficultyOverrides.Current.Value;

            updatingControls = false;
        }

        private void setBool(bool value, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, bool> setter)
        {
            if (updatingControls)
                return;

            model.SetSelectionBoolSetting(setter, value);
            updateFromSelection();
        }

        private void setFloat(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, float> setter)
        {
            if (updatingControls)
                return;

            if (!float.TryParse(source.Current.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return;

            model.SetSelectionFloatSetting(setter, value);
            updateFromSelection();
        }

        private void setInt(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, int> setter)
        {
            if (updatingControls)
                return;

            if (!int.TryParse(source.Current.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return;

            model.SetSelectionIntSetting(setter, value);
            updateFromSelection();
        }

        private static FillFlowContainer createContainer(params Drawable[] children)
            => new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding { Left = 15 },
                Spacing = new Vector2(5),
                Children = children,
            };

        private static void setEnabledState(bool enabled, params Drawable[] items)
        {
            foreach (var item in items)
            {
                switch (item)
                {
                    case FormCheckBox c:
                        c.Current.Disabled = !enabled;
                        break;

                    case FormTextBox t:
                        t.Current.Disabled = !enabled;
                        break;

                    case FormButton b:
                        b.Enabled.Value = enabled;
                        break;
                }
            }
        }

        private static string formatFloat(float value)
            => float.IsNaN(value) ? string.Empty : value.ToString(CultureInfo.InvariantCulture);

        private static string formatInt(int value)
            => value.ToString(CultureInfo.InvariantCulture);
    }
}
