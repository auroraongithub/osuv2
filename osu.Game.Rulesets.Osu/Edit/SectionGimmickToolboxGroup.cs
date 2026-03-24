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
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class SectionGimmickToolboxGroup : EditorToolboxGroup
    {
        private readonly BindableInt selectedSectionId = new BindableInt(-1);

        [Resolved(canBeNull: true)]
        private SectionGimmickEditorModel? resolvedModel { get; set; }

        private SectionGimmickEditorModel model = null!;

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

        private SectionSelectionDropdown sectionDropdown = null!;

        private FormTextBox sectionNameBox = null!;
        private FormNumberBox startTimeBox = null!;
        private FormNumberBox endTimeBox = null!;

        private FormButton setStartHereButton = null!;
        private FormButton setEndHereButton = null!;
        private FormButton setGradualFinishTimeButton = null!;

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
        private FormNumberBox maxMisses = null!;

        private FormCheckBox enableNoMissedSliderEnd = null!;

        private FormCheckBox enableGreatOffsetPenalty = null!;
        private FillFlowContainer greatOffsetFields = null!;
        private FormNumberBox greatOffsetThreshold = null!;
        private FormNumberBox greatOffsetPenaltyHp = null!;

        private FormCheckBox enableDifficultyOverrides = null!;
        private FillFlowContainer difficultyOverrideFields = null!;
        private FormCheckBox enableGradualDifficultyChange = null!;
        private FormNumberBox gradualDifficultyChangeEndTime = null!;
        private FormCheckBox keepDifficultyOverridesAfterSection = null!;
        private FormButton inheritFromPreviousButton = null!;
        private FormNumberBox sectionCircleSize = null!;
        private FormNumberBox sectionApproachRate = null!;
        private FormNumberBox sectionOverallDifficulty = null!;

        private FormCheckBox forceHidden = null!;
        private FormCheckBox forceNoApproachCircle = null!;
        private FormCheckBox forceHardRock = null!;
        private FormCheckBox forceFlashlight = null!;
        private FormCheckBox forceDoubleTime = null!;
        private FormCheckBox forceSingleTap = null!;
        private FormCheckBox forceAlternate = null!;

        private FormCheckBox showFunMods = null!;
        private FillFlowContainer funModsPanel = null!;
        private FormCheckBox forceTransform = null!;
        private FormCheckBox forceWiggle = null!;
        private FormCheckBox forceSpinIn = null!;
        private FormCheckBox forceGrow = null!;
        private FormCheckBox forceDeflate = null!;
        private FormCheckBox forceBarrelRoll = null!;
        private FormCheckBox forceApproachDifferent = null!;
        private FormCheckBox forceMuted = null!;
        private FormCheckBox forceNoScope = null!;
        private FormCheckBox forceMagnetised = null!;
        private FormCheckBox forceRepel = null!;
        private FormCheckBox forceFreezeFrame = null!;
        private FormCheckBox forceBubbles = null!;
        private FormCheckBox forceSynesthesia = null!;
        private FormCheckBox forceDepth = null!;
        private FormCheckBox forceBloom = null!;

        // Fun mod adjustable value controls (shown when corresponding mod is enabled)
        private FillFlowContainer wiggleSettings = null!;
        private FormSliderBar<float> wiggleStrength = null!;
        private FillFlowContainer growSettings = null!;
        private FormSliderBar<float> growStartScale = null!;
        private FillFlowContainer deflateSettings = null!;
        private FormSliderBar<float> deflateStartScale = null!;
        private FillFlowContainer approachDifferentSettings = null!;
        private FormSliderBar<float> approachDifferentScale = null!;
        private FillFlowContainer noScopeSettings = null!;
        private FormSliderBar<int> noScopeHiddenComboCount = null!;
        private FillFlowContainer magnetisedSettings = null!;
        private FormSliderBar<float> magnetisedAttractionStrength = null!;
        private FillFlowContainer repelSettings = null!;
        private FormSliderBar<float> repelRepulsionStrength = null!;
        private FillFlowContainer depthSettings = null!;
        private FormSliderBar<float> depthMaxDepth = null!;
        private FillFlowContainer bloomSettings = null!;
        private FormSliderBar<int> bloomMaxSizeComboCount = null!;
        private FormSliderBar<float> bloomMaxCursorSize = null!;
        private FillFlowContainer barrelRollSettings = null!;
        private FormSliderBar<double> barrelRollSpinSpeed = null!;
        private FillFlowContainer mutedSettings = null!;
        private FormSliderBar<int> mutedMuteComboCount = null!;

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
            model = resolvedModel ?? new SectionGimmickEditorModel(editorBeatmap);
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
                    sectionDropdown = new SectionSelectionDropdown
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Sections",
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
                                    maxMisses = new FormNumberBox
                                    {
                                        Caption = "MaxMisses (-1 unlimited)",
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
                                    enableGradualDifficultyChange = new FormCheckBox
                                    {
                                        Caption = "Gradual change",
                                    },
                                    gradualDifficultyChangeEndTime = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "Gradual finish time (ms)",
                                        TabbableContentContainer = this,
                                    },
                                    setGradualFinishTimeButton = new FormButton
                                    {
                                        Caption = "Gradual finish time (ms)",
                                        ButtonText = "Set Finish Here",
                                        Action = () => mutateSetting(s => s.GradualDifficultyChangeEndTimeMs = (float)clock.CurrentTime),
                                    },
                                    keepDifficultyOverridesAfterSection = new FormCheckBox
                                    {
                                        Caption = "Keep overrides after section",
                                    },
                                    inheritFromPreviousButton = new FormButton
                                    {
                                        Caption = "Difficulty Overrides",
                                        ButtonText = "Inherit from Previous",
                                        Action = inheritDifficultyFromPrevious,
                                    },
                                    sectionCircleSize = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "SectionCircleSize (0-11)",
                                        TabbableContentContainer = this,
                                    },
                                    sectionApproachRate = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "SectionApproachRate (<= 11)",
                                        TabbableContentContainer = this,
                                    },
                                    sectionOverallDifficulty = new FormNumberBox(allowDecimals: true)
                                    {
                                        Caption = "SectionOverallDifficulty (0-11)",
                                        TabbableContentContainer = this,
                                    },
                                }
                            },

                            forceHidden = new FormCheckBox
                            {
                                Caption = "Force Hidden (HD)",
                            },
                            forceNoApproachCircle = new FormCheckBox
                            {
                                Caption = "Force No Approach Circle",
                            },
                            forceHardRock = new FormCheckBox
                            {
                                Caption = "Force Hard Rock (HR)",
                            },
                            forceFlashlight = new FormCheckBox
                            {
                                Caption = "Force Flashlight (FL)",
                            },
                            forceDoubleTime = new FormCheckBox
                            {
                                Caption = "Force Double Time (DT)",
                            },
                            forceSingleTap = new FormCheckBox
                            {
                                Caption = "Force Single Tap (SG)",
                            },
                            forceAlternate = new FormCheckBox
                            {
                                Caption = "Force Alternate (AL)",
                            },

                            showFunMods = new FormCheckBox
                            {
                                Caption = "Show Fun Mods (collapsible)",
                            },
                            funModsPanel = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Alpha = 0,
                                AlwaysPresent = true,
                                Children = new Drawable[]
                                {
                                    forceTransform = new FormCheckBox
                                    {
                                        Caption = "Force Transform (TR)",
                                    },
                                    forceWiggle = new FormCheckBox
                                    {
                                        Caption = "Force Wiggle (WG)",
                                    },
                                    forceSpinIn = new FormCheckBox
                                    {
                                        Caption = "Force Spin In (SI)",
                                    },
                                    forceGrow = new FormCheckBox
                                    {
                                        Caption = "Force Grow (GR)",
                                    },
                                    forceDeflate = new FormCheckBox
                                    {
                                        Caption = "Force Deflate (DF)",
                                    },
                                    forceBarrelRoll = new FormCheckBox
                                    {
                                        Caption = "Force Barrel Roll (BR)",
                                    },
                                    forceApproachDifferent = new FormCheckBox
                                    {
                                        Caption = "Force Approach Different (AD)",
                                    },
                                    forceMuted = new FormCheckBox
                                    {
                                        Caption = "Force Muted (MU)",
                                    },
                                    forceNoScope = new FormCheckBox
                                    {
                                        Caption = "Force No Scope (NS)",
                                    },
                                    forceMagnetised = new FormCheckBox
                                    {
                                        Caption = "Force Magnetised (MG)",
                                    },
                                    forceRepel = new FormCheckBox
                                    {
                                        Caption = "Force Repel (RP)",
                                    },
                                    forceFreezeFrame = new FormCheckBox
                                    {
                                        Caption = "Force Freeze Frame (FF)",
                                    },
                                    forceBubbles = new FormCheckBox
                                    {
                                        Caption = "Force Bubbles (BL)",
                                    },
                                    forceSynesthesia = new FormCheckBox
                                    {
                                        Caption = "Force Synesthesia (SY)",
                                    },
                                    forceDepth = new FormCheckBox
                                    {
                                        Caption = "Force Depth (DP)",
                                    },
                                    forceBloom = new FormCheckBox
                                    {
                                        Caption = "Force Bloom (BM)",
                                    },

                                    // Wiggle settings (shown when ForceWiggle is enabled)
                                    wiggleSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            wiggleStrength = new FormSliderBar<float>
                                            {
                                                Caption = "Strength",
                                                Current = new BindableFloat(1) { MinValue = 0.1f, MaxValue = 2f, Precision = 0.1f },
                                            },
                                        }
                                    },

                                    // Grow settings
                                    growSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            growStartScale = new FormSliderBar<float>
                                            {
                                                Caption = "Start Scale",
                                                Current = new BindableFloat(0.5f) { MinValue = 0f, MaxValue = 0.99f, Precision = 0.01f },
                                            },
                                        }
                                    },

                                    // Deflate settings
                                    deflateSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            deflateStartScale = new FormSliderBar<float>
                                            {
                                                Caption = "Start Scale",
                                                Current = new BindableFloat(2) { MinValue = 1f, MaxValue = 25f, Precision = 0.1f },
                                            },
                                        }
                                    },

                                    // ApproachDifferent settings
                                    approachDifferentSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            approachDifferentScale = new FormSliderBar<float>
                                            {
                                                Caption = "Initial Size",
                                                Current = new BindableFloat(4) { MinValue = 1.5f, MaxValue = 10f, Precision = 0.1f },
                                            },
                                        }
                                    },

                                    // NoScope settings
                                    noScopeSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            noScopeHiddenComboCount = new FormSliderBar<int>
                                            {
                                                Caption = "Hidden Combo Count",
                                                Current = new BindableInt(10) { MinValue = 0, MaxValue = 50 },
                                            },
                                        }
                                    },

                                    // Magnetised settings
                                    magnetisedSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            magnetisedAttractionStrength = new FormSliderBar<float>
                                            {
                                                Caption = "Attraction Strength",
                                                Current = new BindableFloat(0.5f) { MinValue = 0.05f, MaxValue = 1f, Precision = 0.05f },
                                            },
                                        }
                                    },

                                    // Repel settings
                                    repelSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            repelRepulsionStrength = new FormSliderBar<float>
                                            {
                                                Caption = "Repulsion Strength",
                                                Current = new BindableFloat(0.5f) { MinValue = 0.05f, MaxValue = 1f, Precision = 0.05f },
                                            },
                                        }
                                    },

                                    // Depth settings
                                    depthSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            depthMaxDepth = new FormSliderBar<float>
                                            {
                                                Caption = "Max Depth",
                                                Current = new BindableFloat(100) { MinValue = 50f, MaxValue = 200f, Precision = 10f },
                                            },
                                        }
                                    },

                                    // Bloom settings
                                    bloomSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            bloomMaxSizeComboCount = new FormSliderBar<int>
                                            {
                                                Caption = "Max Size Combo",
                                                Current = new BindableInt(50) { MinValue = 5, MaxValue = 100 },
                                            },
                                            bloomMaxCursorSize = new FormSliderBar<float>
                                            {
                                                Caption = "Max Cursor Size",
                                                Current = new BindableFloat(10f) { MinValue = 5f, MaxValue = 15f, Precision = 0.5f },
                                            },
                                        }
                                    },

                                    // BarrelRoll settings
                                    barrelRollSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            barrelRollSpinSpeed = new FormSliderBar<double>
                                            {
                                                Caption = "Spin Speed (RPM)",
                                                Current = new BindableDouble(0.5) { MinValue = 0.02, MaxValue = 12, Precision = 0.01 },
                                            },
                                        }
                                    },

                                    // Muted settings
                                    mutedSettings = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Margin = new MarginPadding { Left = 20 },
                                        Children = new Drawable[]
                                        {
                                            mutedMuteComboCount = new FormSliderBar<int>
                                            {
                                                Caption = "Mute Combo Count",
                                                Current = new BindableInt(100) { MinValue = 0, MaxValue = 500 },
                                            },
                                        }
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
            hp100.OnCommit += (_, _) => updateFloatSetting(hp100, (s, v) => s.HP100 = v);
            hp50.OnCommit += (_, _) => updateFloatSetting(hp50, (s, v) => s.HP50 = v);
            hpMiss.OnCommit += (_, _) => updateFloatSetting(hpMiss, (s, v) => s.HPMiss = v);

            enableNoMiss.Current.BindValueChanged(v => mutateSetting(s => s.EnableNoMiss = v.NewValue));

            enableCountLimits.Current.BindValueChanged(v => mutateSetting(s => s.EnableCountLimits = v.NewValue));
            max300s.OnCommit += (_, _) => updateIntSetting(max300s, (s, v) => s.Max300s = v);
            max100s.OnCommit += (_, _) => updateIntSetting(max100s, (s, v) => s.Max100s = v);
            max50s.OnCommit += (_, _) => updateIntSetting(max50s, (s, v) => s.Max50s = v);
            maxMisses.OnCommit += (_, _) => updateIntSetting(maxMisses, (s, v) => s.MaxMisses = v);

            enableNoMissedSliderEnd.Current.BindValueChanged(v => mutateSetting(s => s.EnableNoMissedSliderEnd = v.NewValue));

            enableGreatOffsetPenalty.Current.BindValueChanged(v => mutateSetting(s => s.EnableGreatOffsetPenalty = v.NewValue));
            greatOffsetThreshold.OnCommit += (_, _) => updateFloatSetting(greatOffsetThreshold, (s, v) => s.GreatOffsetThresholdMs = v);
            greatOffsetPenaltyHp.OnCommit += (_, _) => updateFloatSetting(greatOffsetPenaltyHp, (s, v) => s.GreatOffsetPenaltyHP = v);

            enableDifficultyOverrides.Current.BindValueChanged(v => mutateSetting(s => s.EnableDifficultyOverrides = v.NewValue));
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
            forceSingleTap.Current.BindValueChanged(v => mutateSetting(s =>
            {
                s.ForceSingleTap = v.NewValue;
                if (v.NewValue)
                    s.ForceAlternate = false;
            }));
            forceAlternate.Current.BindValueChanged(v => mutateSetting(s =>
            {
                s.ForceAlternate = v.NewValue;
                if (v.NewValue)
                    s.ForceSingleTap = false;
            }));

            showFunMods.Current.BindValueChanged(v =>
            {
                funModsPanel.FadeTo(v.NewValue ? 1 : 0, 200, Easing.OutQuint);
            });

            forceTransform.Current.BindValueChanged(v => mutateSetting(s => s.ForceTransform = v.NewValue));
            forceWiggle.Current.BindValueChanged(v => mutateSetting(s => s.ForceWiggle = v.NewValue));
            forceSpinIn.Current.BindValueChanged(v => mutateSetting(s => s.ForceSpinIn = v.NewValue));
            forceGrow.Current.BindValueChanged(v => mutateSetting(s => s.ForceGrow = v.NewValue));
            forceDeflate.Current.BindValueChanged(v => mutateSetting(s => s.ForceDeflate = v.NewValue));
            forceBarrelRoll.Current.BindValueChanged(v => mutateSetting(s => s.ForceBarrelRoll = v.NewValue));
            forceApproachDifferent.Current.BindValueChanged(v => mutateSetting(s => s.ForceApproachDifferent = v.NewValue));
            forceMuted.Current.BindValueChanged(v => mutateSetting(s => s.ForceMuted = v.NewValue));
            forceNoScope.Current.BindValueChanged(v => mutateSetting(s => s.ForceNoScope = v.NewValue));
            forceMagnetised.Current.BindValueChanged(v => mutateSetting(s => s.ForceMagnetised = v.NewValue));
            forceRepel.Current.BindValueChanged(v => mutateSetting(s => s.ForceRepel = v.NewValue));
            forceFreezeFrame.Current.BindValueChanged(v => mutateSetting(s => s.ForceFreezeFrame = v.NewValue));
            forceBubbles.Current.BindValueChanged(v => mutateSetting(s => s.ForceBubbles = v.NewValue));
            forceSynesthesia.Current.BindValueChanged(v => mutateSetting(s => s.ForceSynesthesia = v.NewValue));
            forceDepth.Current.BindValueChanged(v => mutateSetting(s => s.ForceDepth = v.NewValue));
            forceBloom.Current.BindValueChanged(v =>
            {
                mutateSetting(s => s.ForceBloom = v.NewValue);
                updateFunModSettingsVisibility();
            });

            // Fun mod settings bindings - also toggle visibility when corresponding checkbox changes
            forceWiggle.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            wiggleStrength.Current.BindValueChanged(v => mutateSetting(s => s.WiggleStrength = v.NewValue));

            forceGrow.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            growStartScale.Current.BindValueChanged(v => mutateSetting(s => s.GrowStartScale = v.NewValue));

            forceDeflate.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            deflateStartScale.Current.BindValueChanged(v => mutateSetting(s => s.DeflateStartScale = v.NewValue));

            forceApproachDifferent.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            approachDifferentScale.Current.BindValueChanged(v => mutateSetting(s => s.ApproachDifferentScale = v.NewValue));

            forceNoScope.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            noScopeHiddenComboCount.Current.BindValueChanged(v => mutateSetting(s => s.NoScopeHiddenComboCount = v.NewValue));

            forceMagnetised.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            magnetisedAttractionStrength.Current.BindValueChanged(v => mutateSetting(s => s.MagnetisedAttractionStrength = v.NewValue));

            forceRepel.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            repelRepulsionStrength.Current.BindValueChanged(v => mutateSetting(s => s.RepelRepulsionStrength = v.NewValue));

            forceDepth.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            depthMaxDepth.Current.BindValueChanged(v => mutateSetting(s => s.DepthMaxDepth = v.NewValue));

            forceBloom.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            bloomMaxSizeComboCount.Current.BindValueChanged(v => mutateSetting(s => s.BloomMaxSizeComboCount = v.NewValue));
            bloomMaxCursorSize.Current.BindValueChanged(v => mutateSetting(s => s.BloomMaxCursorSize = v.NewValue));

            forceBarrelRoll.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            barrelRollSpinSpeed.Current.BindValueChanged(v => mutateSetting(s => s.BarrelRollSpinSpeed = v.NewValue));

            forceMuted.Current.BindValueChanged(v => updateFunModSettingsVisibility());
            mutedMuteComboCount.Current.BindValueChanged(v => mutateSetting(s => s.MutedMuteComboCount = v.NewValue));
        }

        private void mutateSetting(Action<SectionGimmickSettings> settingMutation)
        {
            if (updatingControls)
                return;

            model.SetSelectedSetting(settingMutation);
        }

        private void updateSectionDropdown()
        {
            var sections = model.Sections.OrderBy(s => s.StartTime).ToList();

            updatingControls = true;
            sectionDropdown.Items = sections;

            var selected = sections.FirstOrDefault(s => s.Id == selectedSectionId.Value);
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
                hp100.Current.Value = formatFloat(settings.HP100);
                hp50.Current.Value = formatFloat(settings.HP50);
                hpMiss.Current.Value = formatFloat(settings.HPMiss);

                enableNoMiss.Current.Value = settings.EnableNoMiss;

                enableCountLimits.Current.Value = settings.EnableCountLimits;
                max300s.Current.Value = settings.Max300s.ToString(CultureInfo.InvariantCulture);
                max100s.Current.Value = settings.Max100s.ToString(CultureInfo.InvariantCulture);
                max50s.Current.Value = settings.Max50s.ToString(CultureInfo.InvariantCulture);
                maxMisses.Current.Value = settings.MaxMisses.ToString(CultureInfo.InvariantCulture);

                enableNoMissedSliderEnd.Current.Value = settings.EnableNoMissedSliderEnd;

                enableGreatOffsetPenalty.Current.Value = settings.EnableGreatOffsetPenalty;
                greatOffsetThreshold.Current.Value = formatFloat(settings.GreatOffsetThresholdMs);
                greatOffsetPenaltyHp.Current.Value = formatFloat(settings.GreatOffsetPenaltyHP);

                enableDifficultyOverrides.Current.Value = settings.EnableDifficultyOverrides;
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
                forceSingleTap.Current.Value = settings.ForceSingleTap;
                forceAlternate.Current.Value = settings.ForceAlternate;

                // Fun mods - only update checkbox state, don't auto-show panel
                // The user controls panel visibility via the checkbox
                bool anyFunMod = settings.ForceTransform || settings.ForceWiggle || settings.ForceSpinIn
                    || settings.ForceGrow || settings.ForceDeflate || settings.ForceBarrelRoll
                    || settings.ForceApproachDifferent || settings.ForceMuted || settings.ForceNoScope
                    || settings.ForceMagnetised || settings.ForceRepel || settings.ForceFreezeFrame
                    || settings.ForceBubbles || settings.ForceSynesthesia || settings.ForceDepth || settings.ForceBloom;
                showFunMods.Current.Value = anyFunMod;

                forceTransform.Current.Value = settings.ForceTransform;
                forceWiggle.Current.Value = settings.ForceWiggle;
                forceSpinIn.Current.Value = settings.ForceSpinIn;
                forceGrow.Current.Value = settings.ForceGrow;
                forceDeflate.Current.Value = settings.ForceDeflate;
                forceBarrelRoll.Current.Value = settings.ForceBarrelRoll;
                forceApproachDifferent.Current.Value = settings.ForceApproachDifferent;
                forceMuted.Current.Value = settings.ForceMuted;
                forceNoScope.Current.Value = settings.ForceNoScope;
                forceMagnetised.Current.Value = settings.ForceMagnetised;
                forceRepel.Current.Value = settings.ForceRepel;
                forceFreezeFrame.Current.Value = settings.ForceFreezeFrame;
                forceBubbles.Current.Value = settings.ForceBubbles;
                forceSynesthesia.Current.Value = settings.ForceSynesthesia;
                forceDepth.Current.Value = settings.ForceDepth;
                forceBloom.Current.Value = settings.ForceBloom;

                // Fun mod adjustable values
                wiggleStrength.Current.Value = settings.WiggleStrength;
                growStartScale.Current.Value = settings.GrowStartScale;
                deflateStartScale.Current.Value = settings.DeflateStartScale;
                approachDifferentScale.Current.Value = settings.ApproachDifferentScale;
                noScopeHiddenComboCount.Current.Value = settings.NoScopeHiddenComboCount;
                magnetisedAttractionStrength.Current.Value = settings.MagnetisedAttractionStrength;
                repelRepulsionStrength.Current.Value = settings.RepelRepulsionStrength;
                depthMaxDepth.Current.Value = settings.DepthMaxDepth;
                bloomMaxSizeComboCount.Current.Value = settings.BloomMaxSizeComboCount;
                bloomMaxCursorSize.Current.Value = settings.BloomMaxCursorSize;
                barrelRollSpinSpeed.Current.Value = settings.BarrelRollSpinSpeed;
                mutedMuteComboCount.Current.Value = settings.MutedMuteComboCount;
            }

            updatingControls = false;

            updateGroupVisibility();
            updateValidationState();
            updateFunModSettingsVisibility();
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

            gradualDifficultyChangeEndTime.FadeTo(enableGradualDifficultyChange.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            gradualDifficultyChangeEndTime.AlwaysPresent = enableGradualDifficultyChange.Current.Value;

            setGradualFinishTimeButton.FadeTo(enableGradualDifficultyChange.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            setGradualFinishTimeButton.AlwaysPresent = enableGradualDifficultyChange.Current.Value;
        }

        private void updateFunModSettingsVisibility()
        {
            if (updatingControls) return;

            wiggleSettings.FadeTo(forceWiggle.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            wiggleSettings.AlwaysPresent = forceWiggle.Current.Value;

            growSettings.FadeTo(forceGrow.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            growSettings.AlwaysPresent = forceGrow.Current.Value;

            deflateSettings.FadeTo(forceDeflate.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            deflateSettings.AlwaysPresent = forceDeflate.Current.Value;

            approachDifferentSettings.FadeTo(forceApproachDifferent.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            approachDifferentSettings.AlwaysPresent = forceApproachDifferent.Current.Value;

            noScopeSettings.FadeTo(forceNoScope.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            noScopeSettings.AlwaysPresent = forceNoScope.Current.Value;

            magnetisedSettings.FadeTo(forceMagnetised.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            magnetisedSettings.AlwaysPresent = forceMagnetised.Current.Value;

            repelSettings.FadeTo(forceRepel.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            repelSettings.AlwaysPresent = forceRepel.Current.Value;

            depthSettings.FadeTo(forceDepth.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            depthSettings.AlwaysPresent = forceDepth.Current.Value;

            bloomSettings.FadeTo(forceBloom.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            bloomSettings.AlwaysPresent = forceBloom.Current.Value;

            barrelRollSettings.FadeTo(forceBarrelRoll.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            barrelRollSettings.AlwaysPresent = forceBarrelRoll.Current.Value;

            mutedSettings.FadeTo(forceMuted.Current.Value ? 1 : 0, 200, Easing.OutQuint);
            mutedSettings.AlwaysPresent = forceMuted.Current.Value;
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
