// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class HitObjectGimmickToolboxGroup : EditorToolboxGroup
    {
        [Resolved]
        private osu.Game.Screens.Edit.EditorBeatmap editorBeatmap { get; set; } = null!;

        private HitObjectGimmickEditorModel model = null!;

        private FormCheckBox forceNoApproachCircle = null!;
        private FormButton applyNoApproachButton = null!;
        private FormButton clearNoApproachButton = null!;

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
                    forceNoApproachCircle = new FormCheckBox
                    {
                        Caption = "Force No Approach Circle",
                    },
                    applyNoApproachButton = new FormButton
                    {
                        Caption = "Selection",
                        ButtonText = "Apply to selection",
                        Action = () => model.SetSelectionForceNoApproachCircle(true),
                    },
                    clearNoApproachButton = new FormButton
                    {
                        Caption = "Selection",
                        ButtonText = "Clear from selection",
                        Action = () => model.SetSelectionForceNoApproachCircle(false),
                    },
                }
            };

            forceNoApproachCircle.Current.BindValueChanged(v =>
            {
                if (updatingControls)
                    return;

                model.SetSelectionForceNoApproachCircle(v.NewValue);
                updateFromSelection();
            });

            editorBeatmap.SelectedHitObjects.BindCollectionChanged((_, _) => updateFromSelection(), true);
            editorBeatmap.HitObjectUpdated += _ => updateFromSelection();
            editorBeatmap.HitObjectAdded += _ => updateFromSelection();
            editorBeatmap.HitObjectRemoved += _ => updateFromSelection();
            editorBeatmap.BeatmapReprocessed += updateFromSelection;
        }

        private void updateFromSelection()
        {
            updatingControls = true;

            bool hasSelection = model.HasSelection;
            applyNoApproachButton.Enabled.Value = hasSelection;
            clearNoApproachButton.Enabled.Value = hasSelection;
            forceNoApproachCircle.Current.Value = hasSelection && model.IsSelectionNoApproachCircleForced;

            updatingControls = false;
        }
    }
}
