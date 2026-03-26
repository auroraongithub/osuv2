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
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.Edit;
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

        private RoundedButton addSectionButton = null!;
        private RoundedButton removeSectionButton = null!;
        private CompositeDrawable sectionActionButtons = null!;
        private RoundedButton copySettingsButton = null!;
        private RoundedButton pasteSettingsButton = null!;
        private RoundedButton applyScopeButton = null!;
        private CompositeDrawable settingsActionButtons = null!;

        private SectionSelectionDropdown sectionDropdown = null!;

        private FormTextBox sectionNameBox = null!;
        private FormNumberBox startTimeBox = null!;
        private FormNumberBox endTimeBox = null!;

        private RoundedButton setStartHereButton = null!;
        private RoundedButton setEndHereButton = null!;
        private RoundedButton setGradualFinishTimeButton = null!;

        private FillFlowContainer selectedSectionFlow = null!;

        private FormCheckBox enableHpGimmick = null!;
        private FillFlowContainer hpGroupFields = null!;
        private FormCheckBox noDrain = null!;
        private FormCheckBox reverseHp = null!;
        private FormNumberBox hpStart = null!;
        private FormNumberBox hpCap = null!;
        private FormNumberBox hp300 = null!;
        private FormCheckBox hp300AffectsSliderEndsAndTicks = null!;
        private FormNumberBox hp100 = null!;
        private FormCheckBox hp100AffectsSliderEndsAndTicks = null!;
        private FormNumberBox hp50 = null!;
        private FormCheckBox hp50AffectsSliderEndsAndTicks = null!;
        private FormNumberBox hpMiss = null!;
        private FormCheckBox hpMissAffectsSliderEndAndTickMisses = null!;
        private FormCheckBox showHpSliderRouting = null!;
        private FillFlowContainer hpSliderRoutingFields = null!;

        private FormCheckBox enableNoMiss = null!;

        private FormCheckBox enableCountLimits = null!;
        private FillFlowContainer countLimitFields = null!;
        private FormNumberBox max300s = null!;
        private FormCheckBox max300sAffectsSliderEndsAndTicks = null!;
        private FormNumberBox max100s = null!;
        private FormCheckBox max100sAffectsSliderEndsAndTicks = null!;
        private FormNumberBox max50s = null!;
        private FormCheckBox max50sAffectsSliderEndsAndTicks = null!;
        private FormNumberBox maxMisses = null!;
        private FormCheckBox maxMissesAffectsSliderEndAndTickMisses = null!;
        private FormCheckBox showCountSliderRouting = null!;
        private FillFlowContainer countSliderRoutingFields = null!;

        private FormCheckBox enableNoMissedSliderEnd = null!;

        private FormCheckBox enableGreatOffsetPenalty = null!;
        private FillFlowContainer greatOffsetFields = null!;
        private FormNumberBox greatOffsetThreshold = null!;
        private FormNumberBox greatOffsetPenaltyHp = null!;

        private FormCheckBox enableDifficultyOverrides = null!;
        private FillFlowContainer difficultyOverrideFields = null!;
        private FormCheckBox difficultyOverrideStartWithBeatmapValues = null!;
        private FormCheckBox enableGradualDifficultyChange = null!;
        private FormNumberBox gradualDifficultyChangeEndTime = null!;
        private FormCheckBox keepDifficultyOverridesAfterSection = null!;
        private RoundedButton inheritFromPreviousButton = null!;
        private FormNumberBox sectionCircleSize = null!;
        private FormNumberBox sectionApproachRate = null!;
        private FormNumberBox sectionOverallDifficulty = null!;

        private FormCheckBox forceHidden = null!;
        private FormCheckBox forceNoApproachCircle = null!;
        private FormCheckBox forceHardRock = null!;
        private FormCheckBox forceFlashlight = null!;
        private FormCheckBox forceDoubleTime = null!;
        private FormCheckBox showForceMods = null!;
        private FillFlowContainer forceModsFields = null!;

        private FormEnumDropdown<SectionGimmickApplyScope> applyScopeDropdown = null!;

        private OsuSpriteText validationStatus = null!;

        private bool updatingControls;
        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();

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
                    sectionDropdown = new SectionSelectionDropdown
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Sections",
                    },
                    sectionActionButtons = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                        ColumnDimensions = new[] { new Dimension(), new Dimension() },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                            addSectionButton = new RoundedButton
                            {
                                Text = "Add",
                                RelativeSizeAxes = Axes.X,
                                Action = () => model.AddSection(clock.CurrentTime),
                            },
                            removeSectionButton = new RoundedButton
                            {
                                Text = "Remove",
                                RelativeSizeAxes = Axes.X,
                                Action = () => model.RemoveSelectedSection(),
                            },
                            }
                        }
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
                                Caption = "Name",
                                PlaceholderText = "e.g., Kiai time",
                                TabbableContentContainer = this,
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[] { new Dimension(), new Dimension() },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        startTimeBox = new FormNumberBox(allowDecimals: true)
                                        {
                                            Caption = "Start (ms)",
                                            Current = { Value = "0" },
                                            TabbableContentContainer = this,
                                        },
                                        setStartHereButton = new RoundedButton
                                        {
                                            Text = "Use current time",
                                            RelativeSizeAxes = Axes.X,
                                            Action = () => model.SetSelectedStartTime(clock.CurrentTime),
                                        },
                                    }
                                }
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[] { new Dimension(), new Dimension() },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        endTimeBox = new FormNumberBox(allowDecimals: true)
                                        {
                                            Caption = "End (ms, -1 map end)",
                                            Current = { Value = "-1" },
                                            TabbableContentContainer = this,
                                        },
                                        setEndHereButton = new RoundedButton
                                        {
                                            Text = "Use current time",
                                            RelativeSizeAxes = Axes.X,
                                            Action = () => model.SetSelectedEndTime(clock.CurrentTime),
                                        },
                                    }
                                }
                            },

                            settingsActionButtons = new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[] { new Dimension(), new Dimension() },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        copySettingsButton = new RoundedButton
                                        {
                                            Text = "Copy",
                                            RelativeSizeAxes = Axes.X,
                                            Action = () => model.CopySelectedSettings(),
                                        },
                                        pasteSettingsButton = new RoundedButton
                                        {
                                            Text = "Paste",
                                            RelativeSizeAxes = Axes.X,
                                            Action = () => model.PasteSettingsTo(Array.Empty<int>()),
                                        },
                                    }
                                }
                            },

                            applyScopeDropdown = new FormEnumDropdown<SectionGimmickApplyScope>
                            {
                                Caption = "Apply scope",
                                Current = { Value = SectionGimmickApplyScope.ThisDifficulty },
                            },
                            applyScopeButton = new RoundedButton
                            {
                                Text = "Apply",
                                RelativeSizeAxes = Axes.X,
                                Action = applyCurrentSettingsByScope,
                            },

                            enableHpGimmick = new FormCheckBox
                            {
                                Caption = "HP",
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
                                        Caption = "No drain",
                                    },
                                    reverseHp = new FormCheckBox
                                    {
                                        Caption = "Reverse HP",
                                    },
                                    hpStart = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "HP start (opt. 0-1)",
                                        TabbableContentContainer = this,
                                    },
                                    hpCap = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "HP cap (opt. 0-1)",
                                        TabbableContentContainer = this,
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
                                    showHpSliderRouting = new FormCheckBox
                                    {
                                        Caption = "Show slider HP routing",
                                    },
                                    hpSliderRoutingFields = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Children = new Drawable[]
                                        {
                                            hp300AffectsSliderEndsAndTicks = new FormCheckBox
                                            {
                                                Caption = "Use HP300 for slider hit judgements",
                                            },
                                            hp100AffectsSliderEndsAndTicks = new FormCheckBox
                                            {
                                                Caption = "Use HP100 for slider hit judgements",
                                            },
                                            hp50AffectsSliderEndsAndTicks = new FormCheckBox
                                            {
                                                Caption = "Use HP50 for slider hit judgements",
                                            },
                                            hpMissAffectsSliderEndAndTickMisses = new FormCheckBox
                                            {
                                                Caption = "Use HPMiss for slider miss judgements",
                                            },
                                        }
                                    },
                                }
                            },

                            enableNoMiss = new FormCheckBox
                            {
                                Caption = "No Miss",
                            },

                            enableCountLimits = new FormCheckBox
                            {
                                Caption = "Count limits",
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
                                    maxMisses = new FormNumberBox
                                    {
                                        Caption = "MaxMisses (-1 unlimited)",
                                        TabbableContentContainer = this,
                                    },
                                    showCountSliderRouting = new FormCheckBox
                                    {
                                        Caption = "Show slider count routing",
                                    },
                                    countSliderRoutingFields = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Children = new Drawable[]
                                        {
                                            max300sAffectsSliderEndsAndTicks = new FormCheckBox
                                            {
                                                Caption = "Count slider hits as 300",
                                            },
                                            max100sAffectsSliderEndsAndTicks = new FormCheckBox
                                            {
                                                Caption = "Count slider hits as 100",
                                            },
                                            max50sAffectsSliderEndsAndTicks = new FormCheckBox
                                            {
                                                Caption = "Count slider hits as 50",
                                            },
                                            maxMissesAffectsSliderEndAndTickMisses = new FormCheckBox
                                            {
                                                Caption = "Count slider misses as Miss",
                                            },
                                        }
                                    },
                                }
                            },

                            enableNoMissedSliderEnd = new FormCheckBox
                            {
                                Caption = "No missed slider ends",
                            },

                            enableGreatOffsetPenalty = new FormCheckBox
                            {
                                Caption = "Great offset penalty",
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
                                Caption = "Difficulty overrides (CS/AR/OD)",
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
                                        Caption = "CS (0-11)",
                                        TabbableContentContainer = this,
                                    },
                                    sectionApproachRate = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "AR (<= 11)",
                                        TabbableContentContainer = this,
                                    },
                                    sectionOverallDifficulty = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "OD (0-11)",
                                        TabbableContentContainer = this,
                                    },
                                    difficultyOverrideStartWithBeatmapValues = new FormCheckBox
                                    {
                                        Caption = "Start with beatmap values",
                                    },
                                    enableGradualDifficultyChange = new FormCheckBox
                                    {
                                        Caption = "Gradual change",
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                        ColumnDimensions = new[] { new Dimension(), new Dimension() },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                gradualDifficultyChangeEndTime = new FormNumberBox(allowDecimals: true)
                                                {
                                                    Caption = "Gradual finish time (ms)",
                                                    TabbableContentContainer = this,
                                                },
                                                setGradualFinishTimeButton = new RoundedButton
                                                {
                                                    Text = "Use current time",
                                                    RelativeSizeAxes = Axes.X,
                                                    Action = () => mutateSetting(s => s.GradualDifficultyChangeEndTimeMs = (float)clock.CurrentTime),
                                                },
                                            }
                                        }
                                    },
                                    keepDifficultyOverridesAfterSection = new FormCheckBox
                                    {
                                        Caption = "Keep overrides after section",
                                    },
                                    inheritFromPreviousButton = new RoundedButton
                                    {
                                        Text = "Inherit from previous",
                                        RelativeSizeAxes = Axes.X,
                                        Action = inheritDifficultyFromPrevious,
                                    },
                                }
                            },

                            showForceMods = new FormCheckBox
                            {
                                Caption = "Show forced mods",
                            },
                            forceModsFields = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    forceHidden = new FormCheckBox
                                    {
                                        Caption = "Force HD",
                                    },
                                    forceNoApproachCircle = new FormCheckBox
                                    {
                                        Caption = "Force no approach circle",
                                    },
                                    forceHardRock = new FormCheckBox
                                    {
                                        Caption = "Force HR",
                                    },
                                    forceFlashlight = new FormCheckBox
                                    {
                                        Caption = "Force FL",
                                    },
                                    forceDoubleTime = new FormCheckBox
                                    {
                                        Caption = "Force DT",
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

            updateSectionDropdown();
            updateControlsFromSelection();
        }

        private void bindModelEvents()
        {
            selectedHitObjects.BindTo(editorBeatmap.SelectedHitObjects);
            selectedHitObjects.BindCollectionChanged((_, e) =>
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    if (e.NewItems[e.NewItems.Count - 1] is HitObject mostRecent)
                        trySelectSectionForHitObject(mostRecent);
                }
                else
                {
                    trySelectSectionFromCurrentObjectSelection();
                }
            }, true);

            model.Sections.BindCollectionChanged((_, _) =>
            {
                updateSectionDropdown();
                updateControlsFromSelection();
            }, true);

            selectedSectionId.BindValueChanged(_ => updateControlsFromSelection(), true);

            sectionDropdown.Current.BindValueChanged(v =>
            {
                if (updatingControls)
                    return;

                selectedSectionId.Value = v.NewValue?.Id ?? -1;

                if (v.NewValue != null)
                    clock.SeekSmoothlyTo(v.NewValue.StartTime);
            });
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
            hpStart.OnCommit += (_, _) => updateFloatSetting(hpStart, (s, v) => s.HPStart = v);
            hpCap.OnCommit += (_, _) => updateFloatSetting(hpCap, (s, v) => s.HPCap = v);
            hp100.OnCommit += (_, _) => updateFloatSetting(hp100, (s, v) => s.HP100 = v);
            hp50.OnCommit += (_, _) => updateFloatSetting(hp50, (s, v) => s.HP50 = v);
            hpMiss.OnCommit += (_, _) => updateFloatSetting(hpMiss, (s, v) => s.HPMiss = v);
            hp300AffectsSliderEndsAndTicks.Current.BindValueChanged(v => mutateSetting(s => s.HP300AffectsSliderEndsAndTicks = v.NewValue));
            hp100AffectsSliderEndsAndTicks.Current.BindValueChanged(v => mutateSetting(s => s.HP100AffectsSliderEndsAndTicks = v.NewValue));
            hp50AffectsSliderEndsAndTicks.Current.BindValueChanged(v => mutateSetting(s => s.HP50AffectsSliderEndsAndTicks = v.NewValue));
            hpMissAffectsSliderEndAndTickMisses.Current.BindValueChanged(v => mutateSetting(s => s.HPMissAffectsSliderEndAndTickMisses = v.NewValue));
            showHpSliderRouting.Current.BindValueChanged(_ => updateGroupVisibility());

            enableNoMiss.Current.BindValueChanged(v => mutateSetting(s => s.EnableNoMiss = v.NewValue));

            enableCountLimits.Current.BindValueChanged(v => mutateSetting(s => s.EnableCountLimits = v.NewValue));
            max300s.OnCommit += (_, _) => updateIntSetting(max300s, (s, v) => s.Max300s = v);
            max100s.OnCommit += (_, _) => updateIntSetting(max100s, (s, v) => s.Max100s = v);
            max50s.OnCommit += (_, _) => updateIntSetting(max50s, (s, v) => s.Max50s = v);
            maxMisses.OnCommit += (_, _) => updateIntSetting(maxMisses, (s, v) => s.MaxMisses = v);
            max300sAffectsSliderEndsAndTicks.Current.BindValueChanged(v => mutateSetting(s => s.Max300sAffectsSliderEndsAndTicks = v.NewValue));
            max100sAffectsSliderEndsAndTicks.Current.BindValueChanged(v => mutateSetting(s => s.Max100sAffectsSliderEndsAndTicks = v.NewValue));
            max50sAffectsSliderEndsAndTicks.Current.BindValueChanged(v => mutateSetting(s => s.Max50sAffectsSliderEndsAndTicks = v.NewValue));
            maxMissesAffectsSliderEndAndTickMisses.Current.BindValueChanged(v => mutateSetting(s => s.MaxMissesAffectsSliderEndAndTickMisses = v.NewValue));
            showCountSliderRouting.Current.BindValueChanged(_ => updateGroupVisibility());

            enableNoMissedSliderEnd.Current.BindValueChanged(v => mutateSetting(s => s.EnableNoMissedSliderEnd = v.NewValue));

            enableGreatOffsetPenalty.Current.BindValueChanged(v => mutateSetting(s => s.EnableGreatOffsetPenalty = v.NewValue));
            greatOffsetThreshold.OnCommit += (_, _) => updateFloatSetting(greatOffsetThreshold, (s, v) => s.GreatOffsetThresholdMs = v);
            greatOffsetPenaltyHp.OnCommit += (_, _) => updateFloatSetting(greatOffsetPenaltyHp, (s, v) => s.GreatOffsetPenaltyHP = v);

            enableDifficultyOverrides.Current.BindValueChanged(v => mutateSetting(s => s.EnableDifficultyOverrides = v.NewValue));
            difficultyOverrideStartWithBeatmapValues.Current.BindValueChanged(v => mutateSetting(s => s.DifficultyOverrideStartWithBeatmapValues = v.NewValue));
            enableGradualDifficultyChange.Current.BindValueChanged(v => mutateSetting(s => s.EnableGradualDifficultyChange = v.NewValue));
            gradualDifficultyChangeEndTime.OnCommit += (_, _) => updateFloatSetting(gradualDifficultyChangeEndTime, (s, v) => s.GradualDifficultyChangeEndTimeMs = v);
            keepDifficultyOverridesAfterSection.Current.BindValueChanged(v => mutateSetting(s => s.KeepDifficultyOverridesAfterSection = v.NewValue));
            sectionCircleSize.OnCommit += (_, _) => updateFloatSetting(sectionCircleSize, (s, v) => s.SectionCircleSize = v);
            sectionApproachRate.OnCommit += (_, _) => updateFloatSetting(sectionApproachRate, (s, v) => s.SectionApproachRate = v);
            sectionOverallDifficulty.OnCommit += (_, _) => updateFloatSetting(sectionOverallDifficulty, (s, v) => s.SectionOverallDifficulty = v);

            forceHidden.Current.BindValueChanged(v => mutateSetting(s => s.ForceHidden = v.NewValue));
            forceNoApproachCircle.Current.BindValueChanged(v => mutateSetting(s => s.ForceNoApproachCircle = v.NewValue));
            forceHardRock.Current.BindValueChanged(v => mutateSetting(s => s.ForceHardRock = v.NewValue));
            forceFlashlight.Current.BindValueChanged(v => mutateSetting(s => s.ForceFlashlight = v.NewValue));
            forceDoubleTime.Current.BindValueChanged(v => mutateSetting(s => s.ForceDoubleTime = v.NewValue));
            showForceMods.Current.BindValueChanged(_ => updateGroupVisibility());
        }

        private void mutateSetting(Action<SectionGimmickSettings> settingMutation)
        {
            if (updatingControls)
                return;

            model.SetSelectedSetting(settingMutation);
        }

        private void updateSectionDropdown()
        {
            var orderedSections = model.Sections.OrderBy(s => s.StartTime).ToList();

            SectionGimmickSection? selected = orderedSections.FirstOrDefault(s => s.Id == selectedSectionId.Value);
            var sections = selected == null
                ? orderedSections
                : orderedSections.Where(s => s.Id != selected.Id).Prepend(selected).ToList();

            updatingControls = true;
            sectionDropdown.Items = sections;

            if (selected != null)
                sectionDropdown.Current.Value = selected;
            updatingControls = false;
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

            // Enable "Inherit from Previous" only if there's a previous section with difficulty overrides
            bool canInherit = false;
            if (selected != null)
            {
                var orderedSections = model.Sections.OrderBy(s => s.StartTime).ToList();
                int currentIndex = orderedSections.FindIndex(s => s.Id == selected.Id);
                if (currentIndex > 0)
                {
                    var previous = orderedSections[currentIndex - 1];
                    canInherit = previous.Settings.EnableDifficultyOverrides;
                }
            }
            inheritFromPreviousButton.Enabled.Value = canInherit;

            updatingControls = true;

            if (selected != null)
            {
                sectionDropdown.Current.Value = selected;
                sectionNameBox.Current.Value = selected.Settings.SectionName;

                startTimeBox.Current.Value = selected.StartTime.ToString(CultureInfo.InvariantCulture);
                endTimeBox.Current.Value = selected.EndTime.ToString(CultureInfo.InvariantCulture);

                var settings = selected.Settings;

                enableHpGimmick.Current.Value = settings.EnableHPGimmick;
                noDrain.Current.Value = settings.NoDrain;
                reverseHp.Current.Value = settings.ReverseHP;
                hp300.Current.Value = formatFloat(settings.HP300);
                hpStart.Current.Value = formatFloat(settings.HPStart);
                hpCap.Current.Value = formatFloat(settings.HPCap);
                hp100.Current.Value = formatFloat(settings.HP100);
                hp50.Current.Value = formatFloat(settings.HP50);
                hpMiss.Current.Value = formatFloat(settings.HPMiss);
                hp300AffectsSliderEndsAndTicks.Current.Value = settings.HP300AffectsSliderEndsAndTicks;
                hp100AffectsSliderEndsAndTicks.Current.Value = settings.HP100AffectsSliderEndsAndTicks;
                hp50AffectsSliderEndsAndTicks.Current.Value = settings.HP50AffectsSliderEndsAndTicks;
                hpMissAffectsSliderEndAndTickMisses.Current.Value = settings.HPMissAffectsSliderEndAndTickMisses;
                showHpSliderRouting.Current.Value = settings.HP300AffectsSliderEndsAndTicks
                                                    || settings.HP100AffectsSliderEndsAndTicks
                                                    || settings.HP50AffectsSliderEndsAndTicks
                                                    || settings.HPMissAffectsSliderEndAndTickMisses;

                enableNoMiss.Current.Value = settings.EnableNoMiss;

                enableCountLimits.Current.Value = settings.EnableCountLimits;
                max300s.Current.Value = settings.Max300s.ToString(CultureInfo.InvariantCulture);
                max100s.Current.Value = settings.Max100s.ToString(CultureInfo.InvariantCulture);
                max50s.Current.Value = settings.Max50s.ToString(CultureInfo.InvariantCulture);
                maxMisses.Current.Value = settings.MaxMisses.ToString(CultureInfo.InvariantCulture);
                max300sAffectsSliderEndsAndTicks.Current.Value = settings.Max300sAffectsSliderEndsAndTicks;
                max100sAffectsSliderEndsAndTicks.Current.Value = settings.Max100sAffectsSliderEndsAndTicks;
                max50sAffectsSliderEndsAndTicks.Current.Value = settings.Max50sAffectsSliderEndsAndTicks;
                maxMissesAffectsSliderEndAndTickMisses.Current.Value = settings.MaxMissesAffectsSliderEndAndTickMisses;
                showCountSliderRouting.Current.Value = settings.Max300sAffectsSliderEndsAndTicks
                                                       || settings.Max100sAffectsSliderEndsAndTicks
                                                       || settings.Max50sAffectsSliderEndsAndTicks
                                                       || settings.MaxMissesAffectsSliderEndAndTickMisses;

                enableNoMissedSliderEnd.Current.Value = settings.EnableNoMissedSliderEnd;

                enableGreatOffsetPenalty.Current.Value = settings.EnableGreatOffsetPenalty;
                greatOffsetThreshold.Current.Value = formatFloat(settings.GreatOffsetThresholdMs);
                greatOffsetPenaltyHp.Current.Value = formatFloat(settings.GreatOffsetPenaltyHP);

                enableDifficultyOverrides.Current.Value = settings.EnableDifficultyOverrides;
                difficultyOverrideStartWithBeatmapValues.Current.Value = settings.DifficultyOverrideStartWithBeatmapValues;
                enableGradualDifficultyChange.Current.Value = settings.EnableGradualDifficultyChange;
                gradualDifficultyChangeEndTime.Current.Value = formatFloat(settings.GradualDifficultyChangeEndTimeMs);
                keepDifficultyOverridesAfterSection.Current.Value = settings.KeepDifficultyOverridesAfterSection;
                sectionCircleSize.Current.Value = formatFloat(settings.SectionCircleSize);
                sectionApproachRate.Current.Value = formatFloat(settings.SectionApproachRate);
                sectionOverallDifficulty.Current.Value = formatFloat(settings.SectionOverallDifficulty);

                forceHidden.Current.Value = settings.ForceHidden;
                forceNoApproachCircle.Current.Value = settings.ForceNoApproachCircle;
                forceHardRock.Current.Value = settings.ForceHardRock;
                forceFlashlight.Current.Value = settings.ForceFlashlight;
                forceDoubleTime.Current.Value = settings.ForceDoubleTime;
                showForceMods.Current.Value = settings.ForceHidden
                                               || settings.ForceNoApproachCircle
                                               || settings.ForceHardRock
                                               || settings.ForceFlashlight
                                               || settings.ForceDoubleTime;
            }

            updatingControls = false;

            updateGroupVisibility();
            updateValidationState();
        }

        private void updateGroupVisibility()
        {
            hpGroupFields.FadeTo(enableHpGimmick.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            hpGroupFields.AlwaysPresent = enableHpGimmick.Current.Value;

            hpSliderRoutingFields.FadeTo(enableHpGimmick.Current.Value && showHpSliderRouting.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            hpSliderRoutingFields.AlwaysPresent = enableHpGimmick.Current.Value && showHpSliderRouting.Current.Value;

            countLimitFields.FadeTo(enableCountLimits.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            countLimitFields.AlwaysPresent = enableCountLimits.Current.Value;

            countSliderRoutingFields.FadeTo(enableCountLimits.Current.Value && showCountSliderRouting.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            countSliderRoutingFields.AlwaysPresent = enableCountLimits.Current.Value && showCountSliderRouting.Current.Value;

            greatOffsetFields.FadeTo(enableGreatOffsetPenalty.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            greatOffsetFields.AlwaysPresent = enableGreatOffsetPenalty.Current.Value;

            difficultyOverrideFields.FadeTo(enableDifficultyOverrides.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            difficultyOverrideFields.AlwaysPresent = enableDifficultyOverrides.Current.Value;

            gradualDifficultyChangeEndTime.FadeTo(enableGradualDifficultyChange.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            gradualDifficultyChangeEndTime.AlwaysPresent = enableGradualDifficultyChange.Current.Value;

            setGradualFinishTimeButton.FadeTo(enableGradualDifficultyChange.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            setGradualFinishTimeButton.AlwaysPresent = enableGradualDifficultyChange.Current.Value;

            forceModsFields.FadeTo(showForceMods.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            forceModsFields.AlwaysPresent = showForceMods.Current.Value;
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

        private void trySelectSectionFromCurrentObjectSelection()
        {
            if (updatingControls)
                return;

            if (!selectedHitObjects.Any())
                return;

            var latestSelected = selectedHitObjects.LastOrDefault();
            if (latestSelected == null)
                return;

            trySelectSectionForHitObject(latestSelected);
        }

        private void trySelectSectionForHitObject(HitObject hitObject)
        {
            if (updatingControls)
                return;

            var section = SectionGimmickSectionResolver.Resolve(editorBeatmap.SectionGimmicks, hitObject.StartTime);
            if (section == null)
                return;

            if (selectedSectionId.Value == section.Id)
                return;

            selectedSectionId.Value = section.Id;
            clock.SeekSmoothlyTo(section.StartTime);
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

        private void inheritDifficultyFromPrevious()
        {
            var current = model.Sections.FirstOrDefault(s => s.Id == model.SelectedSectionId.Value);
            if (current == null)
                return;

            var orderedSections = model.Sections.OrderBy(s => s.StartTime).ToList();
            int currentIndex = orderedSections.FindIndex(s => s.Id == current.Id);

            if (currentIndex <= 0)
                return;

            var previous = orderedSections[currentIndex - 1];

            if (!previous.Settings.EnableDifficultyOverrides)
                return;

            mutateSetting(s =>
            {
                s.EnableDifficultyOverrides = true;
                s.SectionCircleSize = previous.Settings.SectionCircleSize;
                s.SectionApproachRate = previous.Settings.SectionApproachRate;
                s.SectionOverallDifficulty = previous.Settings.SectionOverallDifficulty;
            });
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

        private partial class SectionSelectionDropdown : FormDropdown<SectionGimmickSection>
        {
            protected override LocalisableString GenerateItemText(SectionGimmickSection item)
                => string.IsNullOrEmpty(item.Settings.SectionName)
                    ? $"Section {item.Id}"
                    : $"Section {item.Id} - {item.Settings.SectionName}";
        }
    }
}
