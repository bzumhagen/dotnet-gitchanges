using Gitchanges.Changes;
using NUnit.Framework;

namespace Gitchanges.Tests.Changes
{
    [TestFixture]
    public class ChangeVersionTests
    {

        [Test]
        public void CompareTo_HigherVersions_IsLessThan()
        {
            var higherMajorVersion = new ChangeVersion("1.9.0");
            var higherMinorVersion = new ChangeVersion("0.10.0");
            var higherPatchVersion = new ChangeVersion("0.9.1");
            var lowerVersion = new ChangeVersion("0.9.0");
            
            Assert.That(lowerVersion.CompareTo(higherMajorVersion), Is.EqualTo(-1));
            Assert.That(lowerVersion.CompareTo(higherMinorVersion), Is.EqualTo(-1));
            Assert.That(lowerVersion.CompareTo(higherPatchVersion), Is.EqualTo(-1));
        }
        
        [Test]
        public void CompareTo_LowerVersions_IsGreaterThan()
        {
            var lowerMajorVersion = new ChangeVersion("0.10.1");
            var lowerMinorVersion = new ChangeVersion("1.9.1");
            var lowerPatchVersion = new ChangeVersion("1.10.0");
            var higherVersion = new ChangeVersion("1.10.1");
            
            
            Assert.That(higherVersion.CompareTo(lowerMajorVersion), Is.EqualTo(1));
            Assert.That(higherVersion.CompareTo(lowerMinorVersion), Is.EqualTo(1));
            Assert.That(higherVersion.CompareTo(lowerPatchVersion), Is.EqualTo(1));
        }
        
        [Test]
        public void CompareTo_EqualVersions_Equal()
        {
            var versionA = new ChangeVersion("1.10.1");
            var versionB = new ChangeVersion("1.10.1");
            
            Assert.That(versionA.CompareTo(versionB), Is.EqualTo(0));
        }
        
        [Test]
        public void CompareTo_NonNumericVersionAndNumericVersion_NonNumericIsGreaterThan()
        {
            var nonNumeric = new ChangeVersion("Unreleased");
            var numeric = new ChangeVersion("1.10.1");
            
            Assert.That(nonNumeric.CompareTo(numeric), Is.EqualTo(1));
        }
        
        [Test]
        public void CompareTo_NonNumericVersions_GreaterOrdinalIsGreaterThan()
        {
            var nonNumericA = new ChangeVersion("UnreleasedA");
            var nonNumericB = new ChangeVersion("UnreleasedB");
            
            Assert.That(nonNumericB.CompareTo(nonNumericA), Is.EqualTo(1));
        }
    }
}