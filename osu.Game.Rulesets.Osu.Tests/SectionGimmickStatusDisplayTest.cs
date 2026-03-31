// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using NUnit.Framework;
using osu.Game.Beatmaps.SectionGimmicks;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class SectionGimmickStatusDisplayTest
    {
        [Test]
        public void TestBuildLabelWithoutForcedTags()
        {
            var section = new SectionGimmickSection
            {
                Id = 3,
                Settings = new SectionGimmickSettings(),
            };

            string label = invokeBuildLabel(section);
            Assert.That(label, Is.EqualTo("Section 3"));
        }

        [Test]
        public void TestBuildLabelIncludesForcedTags()
        {
            var section = new SectionGimmickSection
            {
                Id = 1,
                Settings = new SectionGimmickSettings
                {
                    SectionName = "Kiai",
                    ForceHidden = true,
                    ForceNoApproachCircle = true,
                    ForceHardRock = true,
                    ForceFlashlight = true,
                    ForceDoubleTime = true,
                    ForceSingleTap = true,
                },
            };

            string label = invokeBuildLabel(section);

            Assert.That(label, Is.EqualTo("Kiai (HD, NoAC, HR, FL, DT, SG)"));
        }

        [Test]
        public void TestBuildLabelIncludesAlternateTag()
        {
            var section = new SectionGimmickSection
            {
                Id = 5,
                Settings = new SectionGimmickSettings
                {
                    ForceAlternate = true,
                },
            };

            string label = invokeBuildLabel(section);

            Assert.That(label, Is.EqualTo("Section 5 (AL)"));
        }

        [Test]
        public void TestBuildLabelIncludesTraceableTag()
        {
            var section = new SectionGimmickSection
            {
                Id = 6,
                Settings = new SectionGimmickSettings
                {
                    ForceTraceable = true,
                },
            };

            string label = invokeBuildLabel(section);

            Assert.That(label, Is.EqualTo("Section 6 (TC)"));
        }

        private static string invokeBuildLabel(SectionGimmickSection section)
        {
            var method = typeof(SectionGimmickStatusDisplay).GetMethod("buildLabel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            return (string)method!.Invoke(null, new object?[] { section })!;
        }
    }
}
