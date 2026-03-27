// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Overlays;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class HitObjectGimmickToolboxGroup : EditorToolboxGroup
    {
        [Resolved]
        private osu.Game.Screens.Edit.EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private INotificationOverlay? notifications { get; set; }

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
        private FormCheckBox allowUnsafeDifficultyOverrideValues = null!;
        private FormNumberBox sectionCircleSize = null!;
        private FormNumberBox sectionApproachRate = null!;
        private FormNumberBox sectionOverallDifficulty = null!;

        private FormCheckBox forceHidden = null!;
        private FormCheckBox forceHardRock = null!;
        private FormCheckBox forceFlashlight = null!;
        private FormNumberBox flashlightRadius = null!;
        private FormCheckBox forceNoApproachCircle = null!;

        private FillFlowContainer hpFields = null!;
        private FillFlowContainer countLimitFields = null!;
        private FillFlowContainer offsetPenaltyFields = null!;
        private FillFlowContainer difficultyOverrideFields = null!;

        private OsuSpriteText selectionStatus = null!;

        private bool updatingControls;
        private readonly ScheduledDelegate[] fadeSchedules = new ScheduledDelegate[4];
        private bool selectionUpdateScheduled;

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
                        allowUnsafeDifficultyOverrideValues = new FormCheckBox { Caption = "Allow values past limits (unsafe)" },
                        sectionCircleSize = new FormNumberBox(allowDecimals: true) { Caption = "SectionCircleSize (0-11)" },
                        sectionApproachRate = new FormNumberBox(allowDecimals: true) { Caption = "SectionApproachRate (<= 11)" },
                        sectionOverallDifficulty = new FormNumberBox(allowDecimals: true) { Caption = "SectionOverallDifficulty (0-11)" }),

                    forceHidden = new FormCheckBox { Caption = "Force Hidden (HD)" },
                    forceHardRock = new FormCheckBox { Caption = "Force Hard Rock (HR)" },
                    forceFlashlight = new FormCheckBox { Caption = "Force Flashlight (FL)" },
                    flashlightRadius = new FormNumberBox(allowDecimals: true) { Caption = "FL radius (20-400)" },
                    forceNoApproachCircle = new FormCheckBox { Caption = "Force No Approach Circle" },
                }
            };

            bindControlEvents();

            editorBeatmap.SelectedHitObjects.BindCollectionChanged((_, _) => scheduleSelectionUpdate(), true);
            editorBeatmap.HitObjectUpdated += _ => scheduleSelectionUpdate();
            editorBeatmap.HitObjectAdded += _ => scheduleSelectionUpdate();
            editorBeatmap.HitObjectRemoved += _ => scheduleSelectionUpdate();
            editorBeatmap.BeatmapReprocessed += scheduleSelectionUpdate;
        }

        private void bindControlEvents()
        {
            enableHpGimmick.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableHPGimmick = value));
            enableNoMiss.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableNoMiss = value));
            enableCountLimits.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableCountLimits = value));
            enableGreatOffsetPenalty.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableGreatOffsetPenalty = value));
            enableDifficultyOverrides.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.EnableDifficultyOverrides = value));
            allowUnsafeDifficultyOverrideValues.Current.BindValueChanged(v =>
            {
                if (!updatingControls && v.NewValue)
                    postUnsafeDifficultyWarning();

                setBool(v.NewValue, (s, value) => s.AllowUnsafeDifficultyOverrideValues = value);
            });

            forceHidden.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceHidden = value));
            forceHardRock.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceHardRock = value));
            forceFlashlight.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceFlashlight = value));
            bindFloatOnCommitOnly(flashlightRadius, (s, value) => s.FlashlightRadius = value, v => Math.Clamp(v, 20f, 400f));
            forceNoApproachCircle.Current.BindValueChanged(v => setBool(v.NewValue, (s, value) => s.ForceNoApproachCircle = value));

            bindFloat(hp300, (s, value) => s.HP300 = value, v => Math.Clamp(v, -2f, 2f));
            bindFloat(hp100, (s, value) => s.HP100 = value, v => Math.Clamp(v, -2f, 2f));
            bindFloat(hp50, (s, value) => s.HP50 = value, v => Math.Clamp(v, -2f, 2f));
            bindFloat(hpMiss, (s, value) => s.HPMiss = value, v => Math.Clamp(v, -2f, 2f));

            bindInt(max300, (s, value) => s.Max300s = value, v => Math.Max(-1, v));
            bindInt(max100, (s, value) => s.Max100s = value, v => Math.Max(-1, v));
            bindInt(max50, (s, value) => s.Max50s = value, v => Math.Max(-1, v));
            bindInt(maxMiss, (s, value) => s.MaxMisses = value, v => Math.Max(-1, v));

            bindFloat(greatOffsetThreshold, (s, value) => s.GreatOffsetThresholdMs = value, v => Math.Max(0f, v));
            bindFloat(greatOffsetPenaltyHp, (s, value) => s.GreatOffsetPenaltyHP = value, v => Math.Min(0f, v));

            bindFloat(sectionCircleSize, (s, value) => s.SectionCircleSize = value, v => isUnsafeDifficultyOverrideEnabled() ? v : SectionGimmickValueClamper.ClampCircleSize(v));
            bindFloat(sectionApproachRate, (s, value) => s.SectionApproachRate = value, v => isUnsafeDifficultyOverrideEnabled() ? v : SectionGimmickValueClamper.ClampApproachRate(v));
            bindFloat(sectionOverallDifficulty, (s, value) => s.SectionOverallDifficulty = value, v => isUnsafeDifficultyOverrideEnabled() ? v : SectionGimmickValueClamper.ClampOverallDifficulty(v));
        }

        private void bindFloat(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, float> setter, Func<float, float> clamp)
            => source.OnCommit += (_, _) => setFloat(source, setter, clamp);

        private void bindFloatOnCommitOnly(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, float> setter, Func<float, float> clamp)
            => source.OnCommit += (_, _) => setFloat(source, setter, clamp);

        private void bindInt(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, int> setter, Func<int, int> clamp)
            => source.OnCommit += (_, _) => setInt(source, setter, clamp);

        private void scheduleSelectionUpdate()
        {
            if (selectionUpdateScheduled)
                return;

            selectionUpdateScheduled = true;
            Scheduler.AddOnce(() =>
            {
                selectionUpdateScheduled = false;
                updateFromSelection();
            });
        }

        private void updateFromSelection()
        {
            updatingControls = true;

            var state = model.GetSelectionState();
            bool hasSelection = state.HasSelection;

            selectionStatus.Text = hasSelection
                ? $"Selected objects: {state.SelectionCount}"
                : "No object selected";
            selectionStatus.Colour = hasSelection ? Color4.White : Color4.Gray;

            // Values may be updated while there is no selection (resetting to defaults).
            // Ensure controls are writable during this update pass, then apply final enabled state below.
            setEnabledState(true,
                enableHpGimmick,
                hp300, hp100, hp50, hpMiss,
                enableNoMiss,
                enableCountLimits, max300, max100, max50, maxMiss,
                enableGreatOffsetPenalty, greatOffsetThreshold, greatOffsetPenaltyHp,
                enableDifficultyOverrides, allowUnsafeDifficultyOverrideValues, sectionCircleSize, sectionApproachRate, sectionOverallDifficulty,
                forceHidden, forceHardRock, forceFlashlight, flashlightRadius, forceNoApproachCircle);

            enableHpGimmick.Current.Value = hasSelection && state.EnableHPGimmick;
            enableNoMiss.Current.Value = hasSelection && state.EnableNoMiss;
            enableCountLimits.Current.Value = hasSelection && state.EnableCountLimits;
            enableGreatOffsetPenalty.Current.Value = hasSelection && state.EnableGreatOffsetPenalty;
            enableDifficultyOverrides.Current.Value = hasSelection && state.EnableDifficultyOverrides;
            allowUnsafeDifficultyOverrideValues.Current.Value = hasSelection && state.AllowUnsafeDifficultyOverrideValues;

            forceHidden.Current.Value = hasSelection && state.ForceHidden;
            forceHardRock.Current.Value = hasSelection && state.ForceHardRock;
            forceFlashlight.Current.Value = hasSelection && state.ForceFlashlight;
            forceNoApproachCircle.Current.Value = hasSelection && state.ForceNoApproachCircle;

            var representative = state.RepresentativeSettings;
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
            flashlightRadius.Current.Value = formatFloat(representative?.FlashlightRadius ?? float.NaN);

            scheduleFade(hpFields, enableHpGimmick.Current.Value, 0);
            hpFields.AlwaysPresent = enableHpGimmick.Current.Value;

            scheduleFade(countLimitFields, enableCountLimits.Current.Value, 1);
            countLimitFields.AlwaysPresent = enableCountLimits.Current.Value;

            scheduleFade(offsetPenaltyFields, enableGreatOffsetPenalty.Current.Value, 2);
            offsetPenaltyFields.AlwaysPresent = enableGreatOffsetPenalty.Current.Value;

            scheduleFade(difficultyOverrideFields, enableDifficultyOverrides.Current.Value, 3);
            difficultyOverrideFields.AlwaysPresent = enableDifficultyOverrides.Current.Value;

            if (IsLoaded)
            {
                bool enabled = hasSelection;
                setEnabledState(enabled,
                    enableHpGimmick,
                    hp300, hp100, hp50, hpMiss,
                    enableNoMiss,
                    enableCountLimits, max300, max100, max50, maxMiss,
                    enableGreatOffsetPenalty, greatOffsetThreshold, greatOffsetPenaltyHp,
                    enableDifficultyOverrides, allowUnsafeDifficultyOverrideValues, sectionCircleSize, sectionApproachRate, sectionOverallDifficulty,
                    forceHidden, forceHardRock, forceFlashlight, flashlightRadius, forceNoApproachCircle);
            }

            updatingControls = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateFromSelection();
        }

        private void scheduleFade(FillFlowContainer container, bool visible, int slot)
        {
            float target = visible ? 1 : 0;

            if (Math.Abs(container.Alpha - target) < 0.0001f)
                return;

            fadeSchedules[slot]?.Cancel();
            fadeSchedules[slot] = Scheduler.AddDelayed(() => container.FadeTo(target, 150), 0);
        }

        private void setBool(bool value, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, bool> setter)
        {
            if (updatingControls)
                return;

            if (!model.HasSelection)
                return;

            model.SetSelectionBoolSetting(setter, value);
            scheduleSelectionUpdate();
        }

        private void setFloat(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, float> setter, Func<float, float> clamp)
        {
            if (updatingControls)
                return;

            if (!model.HasSelection)
                return;

            if (!float.TryParse(source.Current.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return;

            float clamped = clamp(value);
            string formatted = formatFloat(clamped);

            if (source.Current.Value != formatted)
            {
                source.Current.Value = formatted;
                return;
            }

            model.SetSelectionFloatSetting(setter, clamped);
            scheduleSelectionUpdate();
        }

        private void setInt(FormNumberBox source, Action<osu.Game.Beatmaps.HitObjectGimmicks.HitObjectGimmickSettings, int> setter, Func<int, int> clamp)
        {
            if (updatingControls)
                return;

            if (!model.HasSelection)
                return;

            if (!int.TryParse(source.Current.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return;

            int clamped = clamp(value);
            string formatted = formatInt(clamped);

            if (source.Current.Value != formatted)
            {
                source.Current.Value = formatted;
                return;
            }

            model.SetSelectionIntSetting(setter, clamped);
            scheduleSelectionUpdate();
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
                        t.ReadOnly = !enabled;
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

        private bool isUnsafeDifficultyOverrideEnabled()
            => allowUnsafeDifficultyOverrideValues.Current.Value;

        private void postUnsafeDifficultyWarning()
            => notifications?.Post(new SimpleNotification
            {
                Text = "unsafe difficulty overrides enabled - values past normal limits can break gameplay or crash",
            });
    }
}
