// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Applies section-forced fun mods.
    ///
    /// Notes:
    /// - Mods based on per-hitobject drawable callbacks are applied only to drawables
    ///   whose start time belongs to a section that has the corresponding force flag.
    /// - Mods based on playfield-wide update loops (cursor/playfield movement effects)
    ///   run globally once present, matching selected-mod behaviour.
    /// </summary>
    public partial class SectionGimmickFunModsOverlay : CompositeDrawable
    {
        private readonly IBeatmap beatmap;
        private readonly BeatmapSectionGimmicks gimmicks;
        private readonly DrawableRuleset<OsuHitObject> drawableRuleset;
        private readonly IReadOnlyList<Mod> selectedMods;

        private readonly List<ForcedFunMod> forcedMods = new List<ForcedFunMod>();

        private bool drawableModsApplied;

        [Resolved(canBeNull: true)]
        private OsuConfigManager? config { get; set; }

        [Resolved(canBeNull: true)]
        private ScoreProcessor? scoreProcessor { get; set; }

        [Resolved(canBeNull: true)]
        private Player? player { get; set; }

        public SectionGimmickFunModsOverlay(IBeatmap beatmap, DrawableRuleset<OsuHitObject> drawableRuleset, IReadOnlyList<Mod> selectedMods)
        {
            this.beatmap = beatmap;
            this.drawableRuleset = drawableRuleset;
            this.selectedMods = selectedMods;

            gimmicks = beatmap.SectionGimmicks;

            RelativeSizeAxes = Axes.Both;

            initialiseForcedMods();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var forced in forcedMods)
            {
                if (forced.Mod is IReadFromConfig readFromConfig && config != null)
                    readFromConfig.ReadFromConfig(config);

                if (forced.Mod is IApplicableToBeatmap applicableToBeatmap)
                    applicableToBeatmap.ApplyToBeatmap(beatmap);

                if (forced.Mod is IApplicableToDrawableRuleset<OsuHitObject> applicableToDrawableRuleset)
                    applicableToDrawableRuleset.ApplyToDrawableRuleset(drawableRuleset);

                if (forced.Mod is IApplicableToTrack applicableToTrack)
                    applicableToTrack.ApplyToTrack(drawableRuleset.Audio);

                if (forced.Mod is IApplicableToScoreProcessor applicableToScoreProcessor && scoreProcessor != null)
                    applicableToScoreProcessor.ApplyToScoreProcessor(scoreProcessor);

                if (forced.Mod is IApplicableToPlayer applicableToPlayer && player != null)
                    applicableToPlayer.ApplyToPlayer(player);
            }
        }

        protected override void Update()
        {
            base.Update();

            applyDrawableModsOnce();
            updatePlayfieldMods();
        }

        private void applyDrawableModsOnce()
        {
            if (drawableModsApplied)
                return;

            if (!drawableRuleset.Playfield.AllHitObjects.Any())
                return;

            foreach (var forced in forcedMods)
            {
                if (forced.Mod is not IApplicableToDrawableHitObject applicableToDrawableHitObject)
                    continue;

                foreach (var drawable in drawableRuleset.Playfield.AllHitObjects)
                {
                    if (forced.ApplyToAllDrawables || isEnabledAtTime(forced, drawable.HitObject.StartTime))
                        applicableToDrawableHitObject.ApplyToDrawableHitObject(drawable);
                }
            }

            drawableModsApplied = true;
        }

        private void updatePlayfieldMods()
        {
            foreach (var forced in forcedMods)
            {
                if (forced.Mod is not IUpdatableByPlayfield updatableByPlayfield)
                    continue;

                if (forced.AlwaysUpdateWhenPresent || isEnabledAtTime(forced, Time.Current))
                    updatableByPlayfield.Update(drawableRuleset.Playfield);
            }
        }

        private bool isEnabledAtTime(ForcedFunMod forced, double time)
        {
            SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(gimmicks, time);
            return section != null && forced.IsEnabled(section.Settings);
        }

        private void initialiseForcedMods()
        {
            // Get the first section that has each fun mod enabled to get default settings
            SectionGimmickSettings? getFirstSectionWithMod(Func<SectionGimmickSettings, bool> predicate)
                => gimmicks.Sections.FirstOrDefault(s => predicate(s.Settings))?.Settings;

            addIfForced(new OsuModTransform(), s => s.ForceTransform);

            var wiggleMod = new OsuModWiggle();
            var wiggleSettings = getFirstSectionWithMod(s => s.ForceWiggle);
            if (wiggleSettings != null)
                wiggleMod.Strength.Value = wiggleSettings.WiggleStrength;
            addIfForced(wiggleMod, s => s.ForceWiggle);

            addIfForced(new OsuModSpinIn(), s => s.ForceSpinIn);

            var growMod = new OsuModGrow();
            var growSettings = getFirstSectionWithMod(s => s.ForceGrow);
            if (growSettings != null)
                growMod.StartScale.Value = growSettings.GrowStartScale;
            addIfForced(growMod, s => s.ForceGrow);

            var deflateMod = new OsuModDeflate();
            var deflateSettings = getFirstSectionWithMod(s => s.ForceDeflate);
            if (deflateSettings != null)
                deflateMod.StartScale.Value = deflateSettings.DeflateStartScale;
            addIfForced(deflateMod, s => s.ForceDeflate);

            var approachMod = new OsuModApproachDifferent();
            var approachSettings = getFirstSectionWithMod(s => s.ForceApproachDifferent);
            if (approachSettings != null)
                approachMod.Scale.Value = approachSettings.ApproachDifferentScale;
            addIfForced(approachMod, s => s.ForceApproachDifferent);

            addIfForced(new OsuModSynesthesia(), s => s.ForceSynesthesia);
            addIfForced(new OsuModBubbles(), s => s.ForceBubbles);

            // These mods rely on playfield-wide update loops and/or global beatmap adjustments,
            // so run them as global forced effects when present anywhere.
            var barrelRollMod = new OsuModBarrelRoll();
            var brSettings = getFirstSectionWithMod(s => s.ForceBarrelRoll);
            if (brSettings != null)
                barrelRollMod.SpinSpeed.Value = brSettings.BarrelRollSpinSpeed;
            addIfForced(barrelRollMod, s => s.ForceBarrelRoll, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            var mutedMod = new OsuModMuted();
            var mutedSettings = getFirstSectionWithMod(s => s.ForceMuted);
            if (mutedSettings != null)
                mutedMod.MuteComboCount.Value = mutedSettings.MutedMuteComboCount;
            addIfForced(mutedMod, s => s.ForceMuted, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            var noScopeMod = new OsuModNoScope();
            var nsSettings = getFirstSectionWithMod(s => s.ForceNoScope);
            if (nsSettings != null)
                noScopeMod.HiddenComboCount.Value = nsSettings.NoScopeHiddenComboCount;
            addIfForced(noScopeMod, s => s.ForceNoScope, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            var magnetisedMod = new OsuModMagnetised();
            var magSettings = getFirstSectionWithMod(s => s.ForceMagnetised);
            if (magSettings != null)
                magnetisedMod.AttractionStrength.Value = magSettings.MagnetisedAttractionStrength;
            addIfForced(magnetisedMod, s => s.ForceMagnetised, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            var repelMod = new OsuModRepel();
            var repelSettings = getFirstSectionWithMod(s => s.ForceRepel);
            if (repelSettings != null)
                repelMod.RepulsionStrength.Value = repelSettings.RepelRepulsionStrength;
            addIfForced(repelMod, s => s.ForceRepel, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            addIfForced(new OsuModFreezeFrame(), s => s.ForceFreezeFrame, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            var depthMod = new OsuModDepth();
            var depthSettings = getFirstSectionWithMod(s => s.ForceDepth);
            if (depthSettings != null)
                depthMod.MaxDepth.Value = depthSettings.DepthMaxDepth;
            addIfForced(depthMod, s => s.ForceDepth, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);

            var bloomMod = new OsuModBloom();
            var bloomSettings = getFirstSectionWithMod(s => s.ForceBloom);
            if (bloomSettings != null)
            {
                bloomMod.MaxSizeComboCount.Value = bloomSettings.BloomMaxSizeComboCount;
                bloomMod.MaxCursorSize.Value = bloomSettings.BloomMaxCursorSize;
            }
            addIfForced(bloomMod, s => s.ForceBloom, applyToAllDrawables: true, alwaysUpdateWhenPresent: true);
        }

        private void addIfForced(Mod mod, Func<SectionGimmickSettings, bool> enabledPredicate, bool applyToAllDrawables = false, bool alwaysUpdateWhenPresent = false)
        {
            if (selectedMods.Any(m => m.GetType() == mod.GetType()))
                return;

            if (!gimmicks.Sections.Any(s => enabledPredicate(s.Settings)))
                return;

            forcedMods.Add(new ForcedFunMod(mod, enabledPredicate, applyToAllDrawables, alwaysUpdateWhenPresent));
        }

        public static bool HasAnyForcedFunMods(IBeatmap beatmap)
        {
            return beatmap.SectionGimmicks.Sections.Any(s =>
                s.Settings.ForceTransform
                || s.Settings.ForceWiggle
                || s.Settings.ForceSpinIn
                || s.Settings.ForceGrow
                || s.Settings.ForceDeflate
                || s.Settings.ForceBarrelRoll
                || s.Settings.ForceApproachDifferent
                || s.Settings.ForceMuted
                || s.Settings.ForceNoScope
                || s.Settings.ForceMagnetised
                || s.Settings.ForceRepel
                || s.Settings.ForceFreezeFrame
                || s.Settings.ForceBubbles
                || s.Settings.ForceSynesthesia
                || s.Settings.ForceDepth
                || s.Settings.ForceBloom);
        }

        private readonly record struct ForcedFunMod(
            Mod Mod,
            Func<SectionGimmickSettings, bool> IsEnabled,
            bool ApplyToAllDrawables,
            bool AlwaysUpdateWhenPresent);
    }
}
