// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class SectionGimmickToolboxGroup : EditorToolboxGroup
    {
        private readonly BindableInt selectedSectionId = new BindableInt(-1);

        [Resolved]
        private SectionGimmickEditorModel model { get; set; } = null!;

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private Editor? editor { get; set; }

        private FormButton addSectionButton = null!;
        private FormButton removeSectionButton = null!;
        private FormButton copySettingsButton = null!;
        private FormButton pasteSettingsButton = null!;
        private FormButton applyScopeButton = null!;

        private EditorRadioButtonCollection sectionButtons = null!;

        private FormTextBox sectionNameBox = null!;
        private FormNumberBox startTimeBox = null!;
        private FormNumberBox endTimeBox = null!;

        private FormButton setStartHereButton = null!;
        private FormButton setEndHereButton = null!;

        private FillFlowContainer selectedSectionFlow = null!;

        private FormCheckBox enableHpGimmick = null!;
        private FillFlowContainer hpGroupFields = null!;
        private FormCheckBox noDrain = null!;
        private FormCheckBox reverseHp = null!;
        private FormNumberBox hp300 = null!;
        private FormNumberBox hp100 = null!;
        private FormNumberBox hp50 = null!;
        private FormNumberBox hpMiss = null!;

        private FormCheckBox enableNoMiss = null!;

        private FormCheckBox enableCountLimits = null!;
        private FillFlowContainer countLimitFields = null!;
        private FormNumberBox max300s = null!;
        private FormNumberBox max100s = null!;
        private FormNumberBox max50s = null!;

        private FormCheckBox enableNoMissedSliderEnd = null!;

        private FormCheckBox enableGreatOffsetPenalty = null!;
        private FillFlowContainer greatOffsetFields = null!;
        private FormNumberBox greatOffsetThreshold = null!;
        private FormNumberBox greatOffsetPenaltyHp = null!;

        private FormCheckBox enableDifficultyOverrides = null!;
        private FillFlowContainer difficultyOverrideFields = null!;
        private FormNumberBox sectionCircleSize = null!;
        private FormNumberBox sectionApproachRate = null!;
        private FormNumberBox sectionOverallDifficulty = null!;

        private FormEnumDropdown<SectionGimmickApplyScope> applyScopeDropdown = null!;

        private OsuSpriteText validationStatus = null!;

        private bool updatingControls;

        public SectionGimmickToolboxGroup()
            : base("Section gimmicks")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            selectedSectionId.BindTo(model.SelectedSectionId);

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    addSectionButton = new FormButton
                    {
                        Caption = "Sections",
                        ButtonText = "Add Section",
                        Action = () => model.AddSection(clock.CurrentTime),
                    },
                    removeSectionButton = new FormButton
                    {
                        Caption = "Sections",
                        ButtonText = "Remove Selected",
                        Action = () => model.RemoveSelectedSection(),
                    },
                    sectionButtons = new EditorRadioButtonCollection
                    {
                        RelativeSizeAxes = Axes.X,
                    },
                    selectedSectionFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            sectionNameBox = new FormTextBox
                            {
                                Caption = "Section name",
                                PlaceholderText = "e.g., Kiai time",
                                TabbableContentContainer = this,
                            },
                            startTimeBox = new FormNumberBox(allowDecimals: true)
                            {
                                Caption = "Section start (ms)",
                                Current = { Value = "0" },
                                TabbableContentContainer = this,
                            },
                            setStartHereButton = new FormButton
                            {
                                Caption = "Section start (ms)",
                                ButtonText = "Set Start Here",
                                Action = () => model.SetSelectedStartTime(clock.CurrentTime),
                            },
                            endTimeBox = new FormNumberBox(allowDecimals: true)
                            {
                                Caption = "Section end (ms, -1 map end)",
                                Current = { Value = "-1" },
                                TabbableContentContainer = this,
                            },
                            setEndHereButton = new FormButton
                            {
                                Caption = "Section end (ms, -1 map end)",
                                ButtonText = "Set End Here",
                                Action = () => model.SetSelectedEndTime(clock.CurrentTime),
                            },

                            copySettingsButton = new FormButton
                            {
                                Caption = "Settings",
                                ButtonText = "Copy Gimmick Settings",
                                Action = () => model.CopySelectedSettings(),
                            },
                            pasteSettingsButton = new FormButton
                            {
                                Caption = "Settings",
                                ButtonText = "Paste Gimmick Settings",
                                Action = () => model.PasteSettingsTo(Array.Empty<int>()),
                            },

                            applyScopeDropdown = new FormEnumDropdown<SectionGimmickApplyScope>
                            {
                                Caption = "Apply scope",
                                Current = { Value = SectionGimmickApplyScope.ThisDifficulty },
                            },
                            applyScopeButton = new FormButton
                            {
                                Caption = "Apply scope",
                                ButtonText = "Apply Current Settings",
                                Action = applyCurrentSettingsByScope,
                            },

                            enableHpGimmick = new FormCheckBox
                            {
                                Caption = "HP Adjustments",
                            },
                            hpGroupFields = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    noDrain = new FormCheckBox
                                    {
                                        Caption = "NoDrain",
                                    },
                                    reverseHp = new FormCheckBox
                                    {
                                        Caption = "ReverseHP",
                                    },
                                    hp300 = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "HP300",
                                        TabbableContentContainer = this,
                                    },
                                    hp100 = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "HP100",
                                        TabbableContentContainer = this,
                                    },
                                    hp50 = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "HP50",
                                        TabbableContentContainer = this,
                                    },
                                    hpMiss = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "HPMiss",
                                        TabbableContentContainer = this,
                                    },
                                }
                            },

                            enableNoMiss = new FormCheckBox
                            {
                                Caption = "No Miss",
                            },

                            enableCountLimits = new FormCheckBox
                            {
                                Caption = "Count Limits",
                            },
                            countLimitFields = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    max300s = new FormNumberBox
                                    {
                                        Caption = "Max300s (-1 unlimited)",
                                        TabbableContentContainer = this,
                                    },
                                    max100s = new FormNumberBox
                                    {
                                        Caption = "Max100s (-1 unlimited)",
                                        TabbableContentContainer = this,
                                    },
                                    max50s = new FormNumberBox
                                    {
                                        Caption = "Max50s (-1 unlimited)",
                                        TabbableContentContainer = this,
                                    },
                                }
                            },

                            enableNoMissedSliderEnd = new FormCheckBox
                            {
                                Caption = "No Missed Slider End",
                            },

                            enableGreatOffsetPenalty = new FormCheckBox
                            {
                                Caption = "Great Offset Penalty",
                            },
                            greatOffsetFields = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    greatOffsetThreshold = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "GreatOffsetThresholdMs",
                                        TabbableContentContainer = this,
                                    },
                                    greatOffsetPenaltyHp = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "GreatOffsetPenaltyHP",
                                        TabbableContentContainer = this,
                                    },
                                }
                            },

                            enableDifficultyOverrides = new FormCheckBox
                            {
                                Caption = "Difficulty Overrides (CS/AR/OD)",
                            },
                            difficultyOverrideFields = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    sectionCircleSize = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "SectionCircleSize (0-11)",
                                        TabbableContentContainer = this,
                                    },
                                    sectionApproachRate = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "SectionApproachRate (-10 to 11)",
                                        TabbableContentContainer = this,
                                    },
                                    sectionOverallDifficulty = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "SectionOverallDifficulty (0-11)",
                                        TabbableContentContainer = this,
                                    },
                                }
                            },

                            validationStatus = new OsuSpriteText
                            {
                                Text = "Validation: OK",
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                Colour = Color4.White,
                            },
                        }
                    },
                }
            };

            bindModelEvents();
            bindControlEvents();

            updateSectionButtons();
            updateControlsFromSelection();
        }

        private void bindModelEvents()
        {
            model.Sections.BindCollectionChanged((_, _) =>
            {
                updateSectionButtons();
                updateControlsFromSelection();
            }, true);

            selectedSectionId.BindValueChanged(_ => updateControlsFromSelection(), true);
        }

        private void bindControlEvents()
        {
            // Use bindable change instead of commit-only so section names are not lost
            // when user presses apply/save while text box still has focus.
            sectionNameBox.Current.BindValueChanged(v => mutateSetting(s => s.SectionName = v.NewValue));

            startTimeBox.OnCommit += (_, _) =>
            {
                if (tryParseDouble(startTimeBox.Current.Value, out double value))
                    model.SetSelectedStartTime(value);
            };

            endTimeBox.OnCommit += (_, _) =>
            {
                if (tryParseDouble(endTimeBox.Current.Value, out double value))
                    model.SetSelectedEndTime(value);
            };

            enableHpGimmick.Current.BindValueChanged(v => mutateSetting(s => s.EnableHPGimmick = v.NewValue));
            noDrain.Current.BindValueChanged(v => mutateSetting(s => s.NoDrain = v.NewValue));
            reverseHp.Current.BindValueChanged(v => mutateSetting(s => s.ReverseHP = v.NewValue));

            hp300.OnCommit += (_, _) => updateFloatSetting(hp300, (s, v) => s.HP300 = v);
            hp100.OnCommit += (_, _) => updateFloatSetting(hp100, (s, v) => s.HP100 = v);
            hp50.OnCommit += (_, _) => updateFloatSetting(hp50, (s, v) => s.HP50 = v);
            hpMiss.OnCommit += (_, _) => updateFloatSetting(hpMiss, (s, v) => s.HPMiss = v);

            enableNoMiss.Current.BindValueChanged(v => mutateSetting(s => s.EnableNoMiss = v.NewValue));

            enableCountLimits.Current.BindValueChanged(v => mutateSetting(s => s.EnableCountLimits = v.NewValue));
            max300s.OnCommit += (_, _) => updateIntSetting(max300s, (s, v) => s.Max300s = v);
            max100s.OnCommit += (_, _) => updateIntSetting(max100s, (s, v) => s.Max100s = v);
            max50s.OnCommit += (_, _) => updateIntSetting(max50s, (s, v) => s.Max50s = v);

            enableNoMissedSliderEnd.Current.BindValueChanged(v => mutateSetting(s => s.EnableNoMissedSliderEnd = v.NewValue));

            enableGreatOffsetPenalty.Current.BindValueChanged(v => mutateSetting(s => s.EnableGreatOffsetPenalty = v.NewValue));
            greatOffsetThreshold.OnCommit += (_, _) => updateFloatSetting(greatOffsetThreshold, (s, v) => s.GreatOffsetThresholdMs = v);
            greatOffsetPenaltyHp.OnCommit += (_, _) => updateFloatSetting(greatOffsetPenaltyHp, (s, v) => s.GreatOffsetPenaltyHP = v);

            enableDifficultyOverrides.Current.BindValueChanged(v => mutateSetting(s => s.EnableDifficultyOverrides = v.NewValue));
            sectionCircleSize.OnCommit += (_, _) => updateFloatSetting(sectionCircleSize, (s, v) => s.SectionCircleSize = v);
            sectionApproachRate.OnCommit += (_, _) => updateFloatSetting(sectionApproachRate, (s, v) => s.SectionApproachRate = v);
            sectionOverallDifficulty.OnCommit += (_, _) => updateFloatSetting(sectionOverallDifficulty, (s, v) => s.SectionOverallDifficulty = v);
        }

        private void mutateSetting(Action<SectionGimmickSettings> settingMutation)
        {
            if (updatingControls)
                return;

            model.SetSelectedSetting(settingMutation);
        }

        private void updateSectionButtons()
        {
            sectionButtons.Items = model.Sections.Select(s =>
            {
                var button = new RadioButton($"Section {s.Id}", () => selectedSectionId.Value = s.Id)
                {
                    TooltipText = buildSectionTooltip(s),
                };

                button.Selected.Value = s.Id == selectedSectionId.Value;
                return button;
            }).ToArray();
        }

        private LocalisableString buildSectionTooltip(SectionGimmickSection section)
        {
            string end = section.EndTime < 0 ? "map end" : section.EndTime.ToString(CultureInfo.InvariantCulture);
            return $"{section.StartTime.ToString(CultureInfo.InvariantCulture)}ms → {end}";
        }

        private void updateControlsFromSelection()
        {
            var selected = model.Sections.FirstOrDefault(s => s.Id == selectedSectionId.Value);
            bool hasSelection = selected != null;

            removeSectionButton.Enabled.Value = hasSelection;
            selectedSectionFlow.FadeTo(hasSelection ? 1 : 0, 200, Easing.OutQuint);
            selectedSectionFlow.AlwaysPresent = true;

            pasteSettingsButton.Enabled.Value = hasSelection && model.HasCopiedSettings;

            updatingControls = true;

            if (selected != null)
            {
                sectionNameBox.Current.Value = selected.Settings.SectionName;

                startTimeBox.Current.Value = selected.StartTime.ToString(CultureInfo.InvariantCulture);
                endTimeBox.Current.Value = selected.EndTime.ToString(CultureInfo.InvariantCulture);

                var settings = selected.Settings;

                enableHpGimmick.Current.Value = settings.EnableHPGimmick;
                noDrain.Current.Value = settings.NoDrain;
                reverseHp.Current.Value = settings.ReverseHP;
                hp300.Current.Value = formatFloat(settings.HP300);
                hp100.Current.Value = formatFloat(settings.HP100);
                hp50.Current.Value = formatFloat(settings.HP50);
                hpMiss.Current.Value = formatFloat(settings.HPMiss);

                enableNoMiss.Current.Value = settings.EnableNoMiss;

                enableCountLimits.Current.Value = settings.EnableCountLimits;
                max300s.Current.Value = settings.Max300s.ToString(CultureInfo.InvariantCulture);
                max100s.Current.Value = settings.Max100s.ToString(CultureInfo.InvariantCulture);
                max50s.Current.Value = settings.Max50s.ToString(CultureInfo.InvariantCulture);

                enableNoMissedSliderEnd.Current.Value = settings.EnableNoMissedSliderEnd;

                enableGreatOffsetPenalty.Current.Value = settings.EnableGreatOffsetPenalty;
                greatOffsetThreshold.Current.Value = formatFloat(settings.GreatOffsetThresholdMs);
                greatOffsetPenaltyHp.Current.Value = formatFloat(settings.GreatOffsetPenaltyHP);

                enableDifficultyOverrides.Current.Value = settings.EnableDifficultyOverrides;
                sectionCircleSize.Current.Value = formatFloat(settings.SectionCircleSize);
                sectionApproachRate.Current.Value = formatFloat(settings.SectionApproachRate);
                sectionOverallDifficulty.Current.Value = formatFloat(settings.SectionOverallDifficulty);
            }

            updatingControls = false;

            updateGroupVisibility();
            updateValidationState();
        }

        private void updateGroupVisibility()
        {
            hpGroupFields.FadeTo(enableHpGimmick.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            hpGroupFields.AlwaysPresent = enableHpGimmick.Current.Value;

            countLimitFields.FadeTo(enableCountLimits.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            countLimitFields.AlwaysPresent = enableCountLimits.Current.Value;

            greatOffsetFields.FadeTo(enableGreatOffsetPenalty.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            greatOffsetFields.AlwaysPresent = enableGreatOffsetPenalty.Current.Value;

            difficultyOverrideFields.FadeTo(enableDifficultyOverrides.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            difficultyOverrideFields.AlwaysPresent = enableDifficultyOverrides.Current.Value;
        }

        private void updateValidationState()
        {
            try
            {
                var cloned = model.CreateClonedCurrentGimmicks();
                SectionGimmicksValidator.Validate(cloned);
                validationStatus.Text = "Validation: OK";
                validationStatus.Colour = Color4.White;
            }
            catch (Exception e)
            {
                validationStatus.Text = $"Validation: {e.Message}";
                validationStatus.Colour = Color4.IndianRed;
            }
        }

        private void updateFloatSetting(FormNumberBox box, Action<SectionGimmickSettings, float> mutation)
        {
            if (!tryParseFloat(box.Current.Value, out float value))
                return;

            mutateSetting(s => mutation(s, value));
        }

        private void updateIntSetting(FormNumberBox box, Action<SectionGimmickSettings, int> mutation)
        {
            if (!int.TryParse(box.Current.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return;

            mutateSetting(s => mutation(s, value));
        }

        private void applyCurrentSettingsByScope()
        {
            // Explicitly flush current section name before apply/scope actions.
            // This avoids losing text if apply is clicked while the text box still has focus.
            mutateSetting(s => s.SectionName = sectionNameBox.Current.Value);

            if (applyScopeDropdown.Current.Value == SectionGimmickApplyScope.ThisDifficulty)
                return;

            editor?.ApplySectionGimmicksToWholeMapset(model.CreateClonedCurrentGimmicks());
        }

        private static string formatFloat(float value)
            => float.IsNaN(value) ? string.Empty : value.ToString(CultureInfo.InvariantCulture);

        private static bool tryParseFloat(string input, out float value)
            => float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        private static bool tryParseDouble(string input, out double value)
            => double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        public enum SectionGimmickApplyScope
        {
            ThisDifficulty,
            WholeMapset,
        }
    }
}
