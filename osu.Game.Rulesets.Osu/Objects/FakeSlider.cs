// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.HitObjectGimmicks;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class FakeSlider : Slider
    {
        public override Judgement CreateJudgement() => new FakeCircleJudgement();

        public FakePunishMode FakePunishMode { get; set; }
        public bool FakePlayHitsound { get; set; }
        public bool FakeAutoHitOnApproachClose { get; set; }
        public bool FakeAutoHitPlayHitsound { get; set; }
        public bool FakeRevealEnabled { get; set; } = true;
        public float FakeRevealRed { get; set; } = 1f;
        public float FakeRevealGreen { get; set; } = 0.3019608f;
        public float FakeRevealBlue { get; set; } = 0.3019608f;
        public float FakeRevealStrength { get; set; } = HitObjectGimmickSettings.DEFAULT_FAKE_REVEAL_STRENGTH;
        public float FakeRevealLeadInStartMs { get; set; } = HitObjectGimmickSettings.DEFAULT_FAKE_REVEAL_LEAD_IN_START_MS;
        public float FakeRevealLeadInLengthMs { get; set; } = HitObjectGimmickSettings.DEFAULT_FAKE_REVEAL_LEAD_IN_LENGTH_MS;
        public float FakeRevealFadeOutStartMs { get; set; } = HitObjectGimmickSettings.DEFAULT_FAKE_REVEAL_FADE_OUT_START_MS;
        public float FakeRevealFadeOutLengthMs { get; set; } = HitObjectGimmickSettings.DEFAULT_FAKE_REVEAL_FADE_OUT_LENGTH_MS;

        protected override void CreateNestedHitObjects(System.Threading.CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            foreach (var nested in NestedHitObjects)
            {
                switch (nested)
                {
                    case SliderHeadCircle head:
                        head.Slider = this;
                        break;

                    case SliderTick tick:
                        tick.Slider = this;
                        break;

                    case SliderRepeat:
                    case SliderTailCircle:
                        break;
                }
            }
        }
    }
}
