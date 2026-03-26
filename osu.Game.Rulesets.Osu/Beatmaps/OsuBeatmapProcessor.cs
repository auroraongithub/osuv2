// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.HitObjectGimmicks;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmapProcessor : BeatmapProcessor
    {
        /// <summary>
        /// The maximum distance between the end of one object and the start of another
        /// which allows the objects to be stacked on top of another.
        /// </summary>
        public const int STACK_DISTANCE = 3;

        public OsuBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PreProcess()
        {
            IHasComboInformation? lastObj = null;

            // For sanity, ensures that both the first hitobject and the first hitobject after a spinner start a new combo.
            // This is normally enforced by the legacy decoder, but is not enforced by the editor.
            foreach (var obj in Beatmap.HitObjects.OfType<IHasComboInformation>())
            {
                if (obj is not Spinner && (lastObj == null || lastObj is Spinner))
                    obj.NewCombo = true;
                lastObj = obj;
            }

            base.PreProcess();
        }

        public override void PostProcess()
        {
            base.PostProcess();

            applySectionDifficultyOverrides(Beatmap);
            applySectionForcedMods(Beatmap);

            ApplyStacking(Beatmap);
        }

        private static void applySectionForcedMods(IBeatmap beatmap)
        {
            var objectSettingsLookup = createObjectSettingsLookup(beatmap.HitObjectGimmicks);

            if (beatmap.SectionGimmicks.Sections.Count == 0)
            {
                foreach (var hitObject in beatmap.HitObjects.OfType<OsuHitObject>())
                {
                    var objectSettings = getObjectSettings(hitObject, objectSettingsLookup);
                    bool objectForceHidden = objectSettings?.ForceHidden == true;
                    bool objectForceHardRock = objectSettings?.ForceHardRock == true;
                    bool objectNoApproach = objectSettings?.ForceNoApproachCircle == true;

                    setHiddenFlagRecursive(hitObject, objectForceHidden);
                    setNoApproachCircleFlagRecursive(hitObject, objectNoApproach);

                    if (objectForceHardRock)
                        applyHardRockTransforms(hitObject);
                    else
                        restoreFromHardRockTransforms(hitObject);

                    if (objectForceHidden)
                        applyHiddenEffect(hitObject);
                }

                return;
            }

            // Apply forced mods to hit objects based on their section
            foreach (var hitObject in beatmap.HitObjects.OfType<OsuHitObject>())
            {
                SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(beatmap.SectionGimmicks, hitObject.StartTime);

                // Always write the flag so drawable layer can reliably detect section-forced HD.
                // Also propagate to nested objects because Hidden is applied per drawable hitobject.
                var objectSettings = getObjectSettings(hitObject, objectSettingsLookup);

                bool objectForceHidden = objectSettings?.ForceHidden == true;
                setHiddenFlagRecursive(hitObject, (section?.Settings.ForceHidden == true) || objectForceHidden);

                bool objectNoApproach = objectSettings?.ForceNoApproachCircle == true;
                bool sectionNoApproach = section?.Settings.ForceNoApproachCircle == true;
                setNoApproachCircleFlagRecursive(hitObject, sectionNoApproach || objectNoApproach);

                bool forceHardRock = section?.Settings.ForceHardRock == true || objectSettings?.ForceHardRock == true;

                // Apply Hard Rock transformations
                if (forceHardRock)
                {
                    applyHardRockTransforms(hitObject);
                }
                else
                {
                    restoreFromHardRockTransforms(hitObject);
                }

                // Apply Hidden effect by modifying TimeFadeIn
                // This makes objects fade out before they're hit (like HD mod)
                if (section?.Settings.ForceHidden == true || objectSettings?.ForceHidden == true)
                {
                    applyHiddenEffect(hitObject);
                }

                // Force Flashlight is visual effect handled by SectionGimmickFlashlightOverlay.
            }

        }

        private static Dictionary<(double StartTime, int ComboIndexWithOffsets), HitObjectGimmickSettings> createObjectSettingsLookup(BeatmapHitObjectGimmicks gimmicks)
        {
            var lookup = new Dictionary<(double, int), HitObjectGimmickSettings>();

            foreach (var entry in gimmicks.Entries)
                lookup[(entry.StartTime, entry.ComboIndexWithOffsets)] = entry.Settings ?? new HitObjectGimmickSettings();

            return lookup;
        }

        private static HitObjectGimmickSettings? getObjectSettings(OsuHitObject hitObject, Dictionary<(double StartTime, int ComboIndexWithOffsets), HitObjectGimmickSettings> lookup)
            => lookup.TryGetValue((hitObject.StartTime, hitObject.ComboIndexWithOffsets), out HitObjectGimmickSettings? settings) ? settings : null;

        private static void setHiddenFlagRecursive(OsuHitObject osuObject, bool hidden)
        {
            osuObject.ForceHidden = hidden;

            foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                setHiddenFlagRecursive(nested, hidden);
        }

        private static void setNoApproachCircleFlagRecursive(OsuHitObject osuObject, bool noApproachCircle)
        {
            osuObject.ForceNoApproachCircle = noApproachCircle;

            foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                setNoApproachCircleFlagRecursive(nested, noApproachCircle);
        }

        private static void applyHiddenEffect(OsuHitObject hitObject)
        {
            // Reuse exact hidden fade-in adjustment logic from OsuModHidden for 1:1 behaviour.
            OsuModHidden.ApplyFadeInAdjustment(hitObject);
        }

        private static void applyHardRockTransforms(OsuHitObject hitObject)
        {
            const float playfield_height = 384;

            if (!hitObject.ForceHardRockBaselinePosition.HasValue)
            {
                hitObject.ForceHardRockBaselinePosition = hitObject.Position;
            }
            else if (hitObject.ForceHardRockIsApplied)
            {
                // If object appears edited while HR is already applied, fold that edit back to baseline space.
                Vector2 expectedCurrentHr = new Vector2(hitObject.ForceHardRockBaselinePosition.Value.X, playfield_height - hitObject.ForceHardRockBaselinePosition.Value.Y);

                if (!approximatelyEqual(hitObject.Position, expectedCurrentHr))
                    hitObject.ForceHardRockBaselinePosition = new Vector2(hitObject.Position.X, playfield_height - hitObject.Position.Y);
            }

            hitObject.Position = new Vector2(hitObject.ForceHardRockBaselinePosition.Value.X, playfield_height - hitObject.ForceHardRockBaselinePosition.Value.Y);
            hitObject.ForceHardRockIsApplied = true;

            if (hitObject is Slider slider)
            {
                if (slider.ForceHardRockBaselinePath == null)
                {
                    slider.ForceHardRockBaselinePath = clonePath(slider.Path);
                }
                else if (slider.ForceHardRockPathIsApplied)
                {
                    // Detect edits while HR is enabled and fold back to baseline path.
                    var expectedHrPath = flipPathRelativeToStart(slider.ForceHardRockBaselinePath);
                    if (!pathApproximatelyEqual(slider.Path, expectedHrPath))
                        slider.ForceHardRockBaselinePath = flipPathRelativeToStart(slider.Path);
                }

                slider.Path = flipPathRelativeToStart(slider.ForceHardRockBaselinePath);
                slider.ForceHardRockPathIsApplied = true;
            }

            static bool approximatelyEqual(Vector2 a, Vector2 b, float epsilon = 0.01f)
                => Math.Abs(a.X - b.X) <= epsilon && Math.Abs(a.Y - b.Y) <= epsilon;

            static SliderPath clonePath(SliderPath source)
            {
                var clone = new SliderPath();
                clone.ControlPoints.AddRange(source.ControlPoints.Select(c => new PathControlPoint(c.Position, c.Type)));
                clone.ExpectedDistance.Value = source.ExpectedDistance.Value;
                return clone;
            }

            static SliderPath flipPathRelativeToStart(SliderPath source)
            {
                var flipped = new SliderPath();

                if (source.ControlPoints.Count == 0)
                    return flipped;

                // Control points are relative to slider start position.
                // HR vertical flip around playfield centre in absolute space becomes sign inversion of relative Y.
                flipped.ControlPoints.AddRange(source.ControlPoints.Select(c => new PathControlPoint(new Vector2(c.Position.X, -c.Position.Y), c.Type)));
                flipped.ExpectedDistance.Value = source.ExpectedDistance.Value;
                return flipped;
            }

            static bool pathApproximatelyEqual(SliderPath a, SliderPath b, float epsilon = 0.01f)
            {
                if (a.ControlPoints.Count != b.ControlPoints.Count)
                    return false;

                for (int i = 0; i < a.ControlPoints.Count; i++)
                {
                    var p1 = a.ControlPoints[i];
                    var p2 = b.ControlPoints[i];

                    if (p1.Type != p2.Type)
                        return false;

                    if (Math.Abs(p1.Position.X - p2.Position.X) > epsilon || Math.Abs(p1.Position.Y - p2.Position.Y) > epsilon)
                        return false;
                }

                return true;
            }
        }

        private static void restoreFromHardRockTransforms(OsuHitObject hitObject)
        {
            if (hitObject.ForceHardRockBaselinePosition.HasValue && hitObject.ForceHardRockIsApplied)
                hitObject.Position = hitObject.ForceHardRockBaselinePosition.Value;

            hitObject.ForceHardRockIsApplied = false;

            if (hitObject is Slider slider && slider.ForceHardRockBaselinePath != null && slider.ForceHardRockPathIsApplied)
                slider.Path = slider.ForceHardRockBaselinePath;

            if (hitObject is Slider slider2)
                slider2.ForceHardRockPathIsApplied = false;
        }

        private static void adjustDifficultyForDoubleTime(OsuHitObject hitObject, ControlPointInfo controlPointInfo)
        {
            // Double Time (DT) = 1.5x playback speed
            // Effects:
            // - AR appears 1.5x faster (less approach time)
            // - OD windows are 2/3 of original
            // - Scroll speed appears 1.5x faster
            //
            // For section gimmicks, we want objects to have DT-like behavior
            // without actually changing the song timing.
            //
            // We achieve this by:
            // - Increasing effective AR (simulating faster approach)
            // - Tightening hit windows (simulating faster timing requirement)
            //
            // The actual implementation would modify how the playfield
            // calculates approach rates and hit windows for these objects.
            //
            // For now, this is a placeholder for the DT section gimmick effect.
        }

        private static void applySectionDifficultyOverrides(IBeatmap beatmap)
        {
            var orderedSections = beatmap.SectionGimmicks.Sections.OrderBy(s => s.StartTime).ToList();
            var baseDifficulty = beatmap.Difficulty;

            // For gradual changes, interpolation should always start from the section-entry baseline.
            var sectionGradualBaselines = new Dictionary<int, BeatmapDifficulty>();

            var objectSettingsLookup = createObjectSettingsLookup(beatmap.HitObjectGimmicks);

            foreach (var hitObject in beatmap.HitObjects.OfType<OsuHitObject>())
            {
                SectionGimmickSection? section = SectionGimmickSectionResolver.Resolve(beatmap.SectionGimmicks, hitObject.StartTime);
                HitObjectGimmickSettings? objectSettings = getObjectSettings(hitObject, objectSettingsLookup);
                var difficulty = beatmap.Difficulty.Clone();

                if (section?.Settings.EnableDifficultyOverrides == true)
                {
                    if (!sectionGradualBaselines.TryGetValue(section.Id, out var sectionBaseline))
                    {
                        sectionBaseline = section.Settings.DifficultyOverrideStartWithBeatmapValues
                            ? baseDifficulty.Clone()
                            : computeSectionInheritedBaseline(orderedSections, section, baseDifficulty);

                        sectionGradualBaselines[section.Id] = sectionBaseline;
                    }

                    applyDifficultyOverridesForTime(section, hitObject.StartTime, difficulty, sectionBaseline);
                }
                else
                {
                    var keepSection = orderedSections
                        .Where(s => s.Settings.EnableDifficultyOverrides)
                        .Where(s => s.EndTime >= 0 && s.EndTime < hitObject.StartTime)
                        .LastOrDefault();

                    if (keepSection?.Settings.KeepDifficultyOverridesAfterSection == true)
                    {
                        applyDifficultyOverridesForTime(keepSection, keepSection.EndTime, difficulty, baseDifficulty, allowGradual: false);
                    }
                    else
                    {
                        difficulty.CircleSize = baseDifficulty.CircleSize;
                        difficulty.ApproachRate = baseDifficulty.ApproachRate;
                        difficulty.OverallDifficulty = baseDifficulty.OverallDifficulty;
                    }
                }

                if ((section?.Settings.ForceHardRock == true) || (objectSettings?.ForceHardRock == true))
                {
                    difficulty.CircleSize = Math.Min(difficulty.CircleSize * 1.3f, 11f);
                    difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * 1.4f, 10f);
                    difficulty.OverallDifficulty = Math.Min(difficulty.OverallDifficulty * 1.4f, 10f);
                }

                applyObjectDifficultyOverrides(objectSettings, difficulty);

                hitObject.ApplyDefaults(beatmap.ControlPointInfo, difficulty);
            }
        }

        private static BeatmapDifficulty computeSectionInheritedBaseline(List<SectionGimmickSection> orderedSections, SectionGimmickSection targetSection, BeatmapDifficulty baseDifficulty)
        {
            var keepSection = orderedSections
                .Where(s => s.Id != targetSection.Id)
                .Where(s => s.Settings.EnableDifficultyOverrides)
                .Where(s => s.EndTime >= 0 && s.EndTime <= targetSection.StartTime)
                .LastOrDefault();

            if (keepSection?.Settings.KeepDifficultyOverridesAfterSection == true)
            {
                var baseline = baseDifficulty.Clone();
                applyDifficultyOverridesForTime(keepSection, keepSection.EndTime, baseline, baseDifficulty, allowGradual: false);
                return baseline;
            }

            return baseDifficulty.Clone();
        }

        private static void applyObjectDifficultyOverrides(HitObjectGimmickSettings? settings, BeatmapDifficulty difficulty)
        {
            if (settings?.EnableDifficultyOverrides != true)
                return;

            if (!float.IsNaN(settings.SectionCircleSize))
                difficulty.CircleSize = settings.SectionCircleSize;

            if (!float.IsNaN(settings.SectionApproachRate))
                difficulty.ApproachRate = settings.SectionApproachRate;

            if (!float.IsNaN(settings.SectionOverallDifficulty))
                difficulty.OverallDifficulty = settings.SectionOverallDifficulty;
        }

        private static void applyDifficultyOverridesForTime(SectionGimmickSection section, double objectTime, BeatmapDifficulty targetDifficulty, IBeatmapDifficultyInfo baseDifficulty, bool allowGradual = true)
        {
            var settings = section.Settings;

            double progress = 1;
            if (allowGradual && settings.EnableGradualDifficultyChange)
            {
                // Calculate the gradual change duration based on section length
                double sectionEnd = section.EndTime >= 0 ? section.EndTime : double.MaxValue;
                double gradualEnd;

                if (float.IsNaN(settings.GradualDifficultyChangeEndTimeMs))
                {
                    // If no explicit gradual end is set, use the section end
                    // This makes the gradual change duration = full section length
                    gradualEnd = sectionEnd;
                }
                else
                {
                    gradualEnd = settings.GradualDifficultyChangeEndTimeMs;

                    // If gradual end is beyond section end, cap it at section end
                    // This prevents gradual changes that extend past the section
                    if (gradualEnd > sectionEnd)
                        gradualEnd = sectionEnd;
                }

                // Ensure gradual end is after section start
                if (gradualEnd > section.StartTime)
                {
                    double gradualDuration = gradualEnd - section.StartTime;
                    if (gradualDuration > 0)
                        progress = Math.Clamp((objectTime - section.StartTime) / gradualDuration, 0, 1);
                }
            }

            if (!float.IsNaN(settings.SectionCircleSize))
                targetDifficulty.CircleSize = interpolate(baseDifficulty.CircleSize, settings.SectionCircleSize, progress, settings.EnableGradualDifficultyChange && allowGradual);

            if (!float.IsNaN(settings.SectionApproachRate))
                targetDifficulty.ApproachRate = interpolate(baseDifficulty.ApproachRate, settings.SectionApproachRate, progress, settings.EnableGradualDifficultyChange && allowGradual);

            if (!float.IsNaN(settings.SectionOverallDifficulty))
                targetDifficulty.OverallDifficulty = interpolate(baseDifficulty.OverallDifficulty, settings.SectionOverallDifficulty, progress, settings.EnableGradualDifficultyChange && allowGradual);

            static float interpolate(float start, float end, double progress, bool quantizeToOneDecimal)
            {
                float value = (float)(start + (end - start) * progress);
                return quantizeToOneDecimal ? MathF.Round(value, 1, MidpointRounding.AwayFromZero) : value;
            }
        }

        internal static void ApplyStacking(IBeatmap beatmap)
        {
            var hitObjects = beatmap.HitObjects as List<OsuHitObject> ?? beatmap.HitObjects.OfType<OsuHitObject>().ToList();

            if (hitObjects.Count > 0)
            {
                // Reset stacking
                foreach (var h in hitObjects)
                    h.StackHeight = 0;

                if (beatmap.BeatmapVersion >= 6)
                    applyStacking(beatmap, hitObjects, 0, hitObjects.Count - 1);
                else
                    applyStackingOld(beatmap, hitObjects);
            }
        }

        private static void applyStacking(IBeatmap beatmap, List<OsuHitObject> hitObjects, int startIndex, int endIndex)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, endIndex);
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            ArgumentOutOfRangeException.ThrowIfNegative(endIndex);

            int extendedEndIndex = endIndex;

            if (endIndex < hitObjects.Count - 1)
            {
                // Extend the end index to include objects they are stacked on
                for (int i = endIndex; i >= startIndex; i--)
                {
                    int stackBaseIndex = i;

                    for (int n = stackBaseIndex + 1; n < hitObjects.Count; n++)
                    {
                        OsuHitObject stackBaseObject = hitObjects[stackBaseIndex];
                        if (stackBaseObject is Spinner) break;

                        OsuHitObject objectN = hitObjects[n];
                        if (objectN is Spinner)
                            continue;

                        double endTime = stackBaseObject.GetEndTime();
                        float stackThreshold = calculateStackThreshold(beatmap, objectN);

                        if (objectN.StartTime - endTime > stackThreshold)
                            // We are no longer within stacking range of the next object.
                            break;

                        if (Vector2Extensions.Distance(stackBaseObject.Position, objectN.Position) < STACK_DISTANCE
                            || (stackBaseObject is Slider && Vector2Extensions.Distance(stackBaseObject.EndPosition, objectN.Position) < STACK_DISTANCE))
                        {
                            stackBaseIndex = n;

                            // HitObjects after the specified update range haven't been reset yet
                            objectN.StackHeight = 0;
                        }
                    }

                    if (stackBaseIndex > extendedEndIndex)
                    {
                        extendedEndIndex = stackBaseIndex;
                        if (extendedEndIndex == hitObjects.Count - 1)
                            break;
                    }
                }
            }

            // Reverse pass for stack calculation.
            int extendedStartIndex = startIndex;

            for (int i = extendedEndIndex; i > startIndex; i--)
            {
                int n = i;
                /* We should check every note which has not yet got a stack.
                 * Consider the case we have two interwound stacks and this will make sense.
                 *
                 * o <-1      o <-2
                 *  o <-3      o <-4
                 *
                 * We first process starting from 4 and handle 2,
                 * then we come backwards on the i loop iteration until we reach 3 and handle 1.
                 * 2 and 1 will be ignored in the i loop because they already have a stack value.
                 */

                OsuHitObject objectI = hitObjects[i];
                if (objectI.StackHeight != 0 || objectI is Spinner) continue;

                float stackThreshold = calculateStackThreshold(beatmap, objectI);

                /* If this object is a hitcircle, then we enter this "special" case.
                 * It either ends with a stack of hitcircles only, or a stack of hitcircles that are underneath a slider.
                 * Any other case is handled by the "is Slider" code below this.
                 */
                if (objectI is HitCircle)
                {
                    while (--n >= 0)
                    {
                        OsuHitObject objectN = hitObjects[n];
                        if (objectN is Spinner) continue;

                        double endTime = objectN.GetEndTime();

                        // truncation to integer is required to match stable
                        // compare https://github.com/peppy/osu-stable-reference/blob/08e3dafd525934cf48880b08e91c24ce4ad8b761/osu!/GameplayElements/HitObjectManager.cs#L1725
                        // - both quantities being subtracted there are integers
                        if ((int)objectI.StartTime - (int)endTime > stackThreshold)
                            // We are no longer within stacking range of the previous object.
                            break;

                        // HitObjects before the specified update range haven't been reset yet
                        if (n < extendedStartIndex)
                        {
                            objectN.StackHeight = 0;
                            extendedStartIndex = n;
                        }

                        /* This is a special case where hticircles are moved DOWN and RIGHT (negative stacking) if they are under the *last* slider in a stacked pattern.
                         *    o==o <- slider is at original location
                         *        o <- hitCircle has stack of -1
                         *         o <- hitCircle has stack of -2
                         */
                        if (objectN is Slider && Vector2Extensions.Distance(objectN.EndPosition, objectI.Position) < STACK_DISTANCE)
                        {
                            int offset = objectI.StackHeight - objectN.StackHeight + 1;

                            for (int j = n + 1; j <= i; j++)
                            {
                                // For each object which was declared under this slider, we will offset it to appear *below* the slider end (rather than above).
                                OsuHitObject objectJ = hitObjects[j];
                                if (Vector2Extensions.Distance(objectN.EndPosition, objectJ.Position) < STACK_DISTANCE)
                                    objectJ.StackHeight -= offset;
                            }

                            // We have hit a slider.  We should restart calculation using this as the new base.
                            // Breaking here will mean that the slider still has StackCount of 0, so will be handled in the i-outer-loop.
                            break;
                        }

                        if (Vector2Extensions.Distance(objectN.Position, objectI.Position) < STACK_DISTANCE)
                        {
                            // Keep processing as if there are no sliders.  If we come across a slider, this gets cancelled out.
                            //NOTE: Sliders with start positions stacking are a special case that is also handled here.

                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
                else if (objectI is Slider)
                {
                    /* We have hit the first slider in a possible stack.
                     * From this point on, we ALWAYS stack positive regardless.
                     */
                    while (--n >= startIndex)
                    {
                        OsuHitObject objectN = hitObjects[n];
                        if (objectN is Spinner) continue;

                        if (objectI.StartTime - objectN.StartTime > stackThreshold)
                            // We are no longer within stacking range of the previous object.
                            break;

                        if (Vector2Extensions.Distance(objectN.EndPosition, objectI.Position) < STACK_DISTANCE)
                        {
                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
            }
        }

        private static void applyStackingOld(IBeatmap beatmap, List<OsuHitObject> hitObjects)
        {
            for (int i = 0; i < hitObjects.Count; i++)
            {
                OsuHitObject currHitObject = hitObjects[i];

                if (currHitObject.StackHeight != 0 && !(currHitObject is Slider))
                    continue;

                double startTime = currHitObject.GetEndTime();
                int sliderStack = 0;

                for (int j = i + 1; j < hitObjects.Count; j++)
                {
                    float stackThreshold = calculateStackThreshold(beatmap, hitObjects[i]);

                    if (hitObjects[j].StartTime - stackThreshold > startTime)
                        break;

                    // The start position of the hitobject, or the position at the end of the path if the hitobject is a slider
                    Vector2 position2 = currHitObject is Slider currSlider
                        ? currSlider.Position + currSlider.Path.PositionAt(1)
                        : currHitObject.Position;

                    // Note the use of `StartTime` in the code below doesn't match stable's use of `EndTime`.
                    // This is because in the stable implementation, `UpdateCalculations` is not called on the inner-loop hitobject (j)
                    // and therefore it does not have a correct `EndTime`, but instead the default of `EndTime = StartTime`.
                    //
                    // Effects of this can be seen on https://osu.ppy.sh/beatmapsets/243#osu/1146 at sliders around 86647 ms, where
                    // if we use `EndTime` here it would result in unexpected stacking.

                    if (Vector2Extensions.Distance(hitObjects[j].Position, currHitObject.Position) < STACK_DISTANCE)
                    {
                        currHitObject.StackHeight++;
                        startTime = hitObjects[j].StartTime;
                    }
                    else if (Vector2Extensions.Distance(hitObjects[j].Position, position2) < STACK_DISTANCE)
                    {
                        // Case for sliders - bump notes down and right, rather than up and left.
                        sliderStack++;
                        hitObjects[j].StackHeight -= sliderStack;
                        startTime = hitObjects[j].StartTime;
                    }
                }
            }
        }

        /// <remarks>
        /// Truncation of <see cref="OsuHitObject.TimePreempt"/> to <see cref="int"/>, as well as keeping the result as <see cref="float"/>, are both done
        /// <a href="https://github.com/peppy/osu-stable-reference/blob/08e3dafd525934cf48880b08e91c24ce4ad8b761/osu!/GameplayElements/HitObjectManager.cs#L1652">
        /// for the purposes of stable compatibility
        /// </a>.
        /// Note that for top-level objects <see cref="OsuHitObject.TimePreempt"/> is supposed to be integral anyway;
        /// see <see cref="OsuHitObject.ApplyDefaultsToSelf"/> using <see cref="IBeatmapDifficultyInfo.DifficultyRangeInt"/> when calculating it.
        /// Slider ticks and end circles are the exception to that, but they do not matter for stacking.
        /// </remarks>
        private static float calculateStackThreshold(IBeatmap beatmap, OsuHitObject hitObject)
            => (int)hitObject.TimePreempt * beatmap.StackLeniency;
    }
}
