using System;
using System.Collections.Generic;
using Gitchanges.Changes;
using Gitchanges.Enumerables;
using NUnit.Framework;

namespace Gitchanges.Tests.Enumerables
{
    [TestFixture]
    public class FilteredChangesTests
    {
        [Test]
        public void VerifyEnumeratorStopsAtMinVersion()
        {
            var now = DateTimeOffset.Now;
            var minVersion = "1.5.0";
            var changeVersion2 = new GitChange("2.0.0", "Added", "Some 2.0.0 Summary", now);
            var changeVersion1Pt5 = new GitChange(minVersion, "Added", "Some 1.5.0 Summary", now.AddDays(-1));
            var changeVersion1 = new GitChange("1.0.0", "Added", "Some 1.0.0 Summary", now.AddDays(-2));
            var changes = new List<IChange> {changeVersion2, changeVersion1Pt5, changeVersion1};
            var expectedChanges = new List<IChange> {changeVersion2, changeVersion1Pt5};
            
            var filteredChanges = new FilteredChanges(changes, minVersion, new HashSet<string>());
            
            Assert.That(filteredChanges, Is.EquivalentTo(expectedChanges));
        }
        
        [Test]
        public void VerifyEnumeratorSkipsExcludedTags()
        {
            var now = DateTimeOffset.Now;
            var excludedTag = "Maintenance";
            var changeVersion2 = new GitChange("2.0.0", "Added", "Some 2.0.0 Summary", now);
            var changeVersion1Pt5 = new GitChange("1.5.0", excludedTag, "Some 1.5.0 Summary", now.AddDays(-1));
            var changeVersion1 = new GitChange("1.0.0", "Added", "Some 1.0.0 Summary", now.AddDays(-2));
            var changes = new List<IChange> {changeVersion2, changeVersion1Pt5, changeVersion1};
            var expectedChanges = new List<IChange> {changeVersion2, changeVersion1};
            
            var filteredChanges = new FilteredChanges(changes, null, new List<string> {excludedTag});
            
            Assert.That(filteredChanges, Is.EquivalentTo(expectedChanges));
        }
    }
}