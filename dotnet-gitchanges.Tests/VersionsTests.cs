using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace dotnet_gitchanges
{
    public class VersionsTests
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
            var expectedChangeKey = new Versions.ChangeKey(expectedChanges.First());
            
            var versions = new Versions();

            foreach (var change in expectedChanges)
            {
                versions.Add(change);
            }
            
            versions.ChangeKeyToChanges.TryGetValue(expectedChangeKey, out var actualChanges);
            
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
        }
        
        [Test]
        public void VerifyMultipleChangesWithDifferentKeysAddedSuccessfully()
        {
            var today = DateTime.Today;
            var expectedChange1 = new GitChange("0.1.0", "Added", "This is a change", today);
            var expectedChange2 = new GitChange("0.1.0", "Removed", "This is a change", today);
            var expectedChangeKey1 = new Versions.ChangeKey(expectedChange1);
            var expectedChangeKey2 = new Versions.ChangeKey(expectedChange2);
            
            var versions = new Versions();

            versions.Add(expectedChange1);
            versions.Add(expectedChange2);
            
            versions.ChangeKeyToChanges.TryGetValue(expectedChangeKey1, out var actualChanges1);
            
            CollectionAssert.Contains(actualChanges1, expectedChange1);
            CollectionAssert.DoesNotContain(actualChanges1, expectedChange2);
            
            versions.ChangeKeyToChanges.TryGetValue(expectedChangeKey2, out var actualChanges2);
            
            CollectionAssert.Contains(actualChanges2, expectedChange2);
            CollectionAssert.DoesNotContain(actualChanges2, expectedChange1);
        }
        
        [Test]
        public void VerifyGetAsValueDictionaryIsSuccessful()
        {
            var expectedChange1 = new GitChange("0.1.0", "Added", "This is a change", DateTime.Today);
            var expectedChange2 = new GitChange("0.1.0", "Removed", "This is a change", DateTime.Today.Subtract(TimeSpan.FromDays(1)));
            
            var versions = new Versions();

            versions.Add(expectedChange1);
            versions.Add(expectedChange2);

            versions.GetAsValueDictionary();
        }
    }
}