// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.SectionGimmicks;

namespace osu.Game.Tests.Beatmaps.SectionGimmicks
{
    [TestFixture]
    public class BeatmapSectionGimmicksTest
    {
        [Test]
        public void TestFindSectionAtStartAndEndBoundariesAreInclusive()
        {
            var gimmicks = new BeatmapSectionGimmicks();
            gimmicks.Sections.Add(new SectionGimmickSection
            {
                Id = 1,
                StartTime = 1000,
                EndTime = 2000,
                Settings = new SectionGimmickSettings(),
            });

            var atStart = gimmicks.FindSectionAt(1000);
            var atEnd = gimmicks.FindSectionAt(2000);

            Assert.That(atStart, Is.Not.Null);
            Assert.That(atStart!.Id, Is.EqualTo(1));

            Assert.That(atEnd, Is.Not.Null);
            Assert.That(atEnd!.Id, Is.EqualTo(1));
        }
    }
}
