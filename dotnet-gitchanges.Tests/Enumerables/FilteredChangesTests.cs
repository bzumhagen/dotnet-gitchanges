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
            var changeVersion2 = new DefaultChange("2.0.0", "Added", "Some 2.0.0 Summary", now);
            var changeVersion1Pt5 = new DefaultChange(minVersion, "Added", "Some 1.5.0 Summary", now.AddDays(-1));
            var changeVersion1 = new DefaultChange("1.0.0", "Added", "Some 1.0.0 Summary", now.AddDays(-2));
            var changes = new List<IChange> {changeVersion2, changeVersion1Pt5, changeVersion1};
            var expectedChanges = new List<IChange> {changeVersion2, changeVersion1Pt5};
            
            var filteredChanges = new FilteredChanges<IChange>(changes, minVersion, new HashSet<string>());
            
            Assert.That(filteredChanges, Is.EquivalentTo(expectedChanges));
        }
        
        [Test]
        public void VerifyEnumeratorSkipsExcludedTags()
        {
            var now = DateTimeOffset.Now;
            var excludedTag = "Maintenance";
            var changeVersion2 = new DefaultChange("2.0.0", "Added", "Some 2.0.0 Summary", now);
            var changeVersion1Pt5 = new DefaultChange("1.5.0", excludedTag, "Some 1.5.0 Summary", now.AddDays(-1));
            var changeVersion1 = new DefaultChange("1.0.0", "Added", "Some 1.0.0 Summary", now.AddDays(-2));
            var changes = new List<IChange> {changeVersion2, changeVersion1Pt5, changeVersion1};
            var expectedChanges = new List<IChange> {changeVersion2, changeVersion1};
            
            var filteredChanges = new FilteredChanges<IChange>(changes, null, new List<string> {excludedTag});
            
            Assert.That(filteredChanges, Is.EquivalentTo(expectedChanges));
        }
    }
}