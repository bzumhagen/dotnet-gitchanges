using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace dotnet_gitchanges.Tests.Caches
{
    [TestFixture]
    public class ChangeCacheTests
    {

        [Test]
        public void VerifyMultipleChangesWithTheSameKeyAddedSuccessfully()
        {
            var today = DateTime.Today;
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.1.0", "Added", "This is a change", today),
                new GitChange("0.1.0", "Added", "This is another change", today)
            };
            var expectedChangeKey = new ChangeCache.ChangeKey(expectedChanges.First());
            
            var changeCache = new ChangeCache();

            foreach (var change in expectedChanges)
            {
                changeCache.Add(change);
            }
            
            changeCache.ChangeKeyToChanges.TryGetValue(expectedChangeKey, out var actualChanges);
            
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
        }
        
        [Test]
        public void VerifyMultipleChangesWithDifferentKeysAddedSuccessfully()
        {
            var today = DateTime.Today;
            var expectedChange1 = new GitChange("0.1.0", "Added", "This is a change", today);
            var expectedChange2 = new GitChange("0.1.0", "Removed", "This is a change", today);
            var expectedChangeKey1 = new ChangeCache.ChangeKey(expectedChange1);
            var expectedChangeKey2 = new ChangeCache.ChangeKey(expectedChange2);
            
            var changeCache = new ChangeCache();

            changeCache.Add(expectedChange1);
            changeCache.Add(expectedChange2);
            
            changeCache.ChangeKeyToChanges.TryGetValue(expectedChangeKey1, out var actualChanges1);
            
            CollectionAssert.Contains(actualChanges1, expectedChange1);
            CollectionAssert.DoesNotContain(actualChanges1, expectedChange2);
            
            changeCache.ChangeKeyToChanges.TryGetValue(expectedChangeKey2, out var actualChanges2);
            
            CollectionAssert.Contains(actualChanges2, expectedChange2);
            CollectionAssert.DoesNotContain(actualChanges2, expectedChange1);
        }
        
        [Test]
        public void VerifyGetAsValueDictionaryIsSuccessful()
        {
            var expectedChange1 = new GitChange("0.2.0", "Added", "This is the latest 0.2.0", DateTime.Today);
            var expectedChange2 = new GitChange("0.2.0", "Removed", "This is the middle 0.2.0", DateTime.Today.AddHours(-2));
            var expectedChange3 = new GitChange("0.2.0", "Removed", "This is the earliest 0.2.0", DateTime.Today.AddHours(-3));
            var expectedChange4 = new GitChange("0.1.0", "Added", "This is a change", DateTime.Today.AddDays(-1));
            var expectedChange5 = new GitChange("0.1.0", "Removed", "This is the middle 0.1.0", DateTime.Today.AddDays(-2));
            var expectedChange6 = new GitChange("0.1.0", "Removed", "This is the earliest 0.1.0", DateTime.Today.AddDays(-3));
            
            var changeCache = new ChangeCache();

            changeCache.Add(expectedChange1);
            changeCache.Add(expectedChange2);
            changeCache.Add(expectedChange3);
            changeCache.Add(expectedChange4);
            changeCache.Add(expectedChange5);
            changeCache.Add(expectedChange6);

            var actualValueDictionary = changeCache.GetAsValueDictionary();
            var actualVersions = (List<Dictionary<string, object>>) actualValueDictionary["versions"];
            var version2 = actualVersions.Find(v => v.ContainsValue("0.2.0"));
            var version2Tags = (List<Dictionary<string, object>>) version2["tags"];
            var version2AddedDictionary = version2Tags.Find(t => t.ContainsValue("Added"));
            var version2AddedChanges = (List<Dictionary<string, object>>) version2AddedDictionary["changes"];
            var version2AddedChangeSummaries = version2AddedChanges.Select(c => c["summary"]);
            var version2RemovedDictionary = version2Tags.Find(t => t.ContainsValue("Removed"));
            var version2RemovedChanges = (List<Dictionary<string, object>>) version2RemovedDictionary["changes"];
            var version2RemovedChangeSummaries = version2RemovedChanges.Select(c => c["summary"]);
            
            Assert.That(version2["date"], Is.EqualTo(expectedChange1.Date.ToString("yyyy-MM-dd")));
            Assert.That(version2AddedChangeSummaries, Is.EquivalentTo(new List<string>{expectedChange1.Summary}));
            Assert.That(version2RemovedChangeSummaries, Is.EquivalentTo(new List<string>{expectedChange2.Summary, expectedChange3.Summary}));
            
            var version1 = actualVersions.Find(v => v.ContainsValue("0.1.0"));
            var version1Tags = (List<Dictionary<string, object>>) version1["tags"];
            var version1AddedDictionary = version1Tags.Find(t => t.ContainsValue("Added"));
            var version1AddedChanges = (List<Dictionary<string, object>>) version1AddedDictionary["changes"];
            var version1AddedChangeSummaries = version1AddedChanges.Select(c => c["summary"]);
            var version1RemovedDictionary = version1Tags.Find(t => t.ContainsValue("Removed"));
            var version1RemovedChanges = (List<Dictionary<string, object>>) version1RemovedDictionary["changes"];
            var version1RemovedChangeSummaries = version1RemovedChanges.Select(c => c["summary"]);
            
            Assert.That(version1["date"], Is.EqualTo(expectedChange4.Date.ToString("yyyy-MM-dd")));
            Assert.That(version1AddedChangeSummaries, Is.EquivalentTo(new List<string>{expectedChange4.Summary}));
            Assert.That(version1RemovedChangeSummaries, Is.EquivalentTo(new List<string>{expectedChange5.Summary, expectedChange6.Summary}));
        }
    }
}