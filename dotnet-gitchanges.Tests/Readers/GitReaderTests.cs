using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using Gitchanges.Readers;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers
{
    [TestFixture]
    public class GitReaderTests
    {
        private readonly ParsingPatterns  _defaultPatterns = new ParsingPatterns
        {
            Reference = "reference:(.*)[\n]?",
            Version = "version:(.*)[\n]?",
            Tag = "tag:(.*)[\n]?"
        };
        [Test]
        public void VerifyReaderReadsFromRepository()
        {
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1))
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(expectedChanges));
            var reader = new GitReader(repoMock.Object, _defaultPatterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            
            Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyUnreleasedCommitsHaveUnreleasedVersion()
        {
            var expectedChanges = new List<IChange>
            {
                new GitChange("Unreleased", "Added", "Some Unreleased Summary", DateTimeOffset.Now),
                new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.AddDays(-1)),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-2))
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(expectedChanges));
            var reader = new GitReader(repoMock.Object, _defaultPatterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            
            Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyReleasedCommitsWithUnreleasedInVersionHaveCorrectVersion()
        {
            var today = DateTimeOffset.Now;
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            var twoDaysAgo = DateTimeOffset.Now.AddDays(-2);
            var changes = new List<IChange>
            {
                new GitChange("0.2.0", "Added", "Some Summary", today),
                new GitChange("Unreleased", "Added", "Some now released Summary", yesterday),
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.2.0", "Added", "Some Summary", today),
                new GitChange("0.2.0", "Added", "Some now released Summary", yesterday),
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(changes));
            var reader = new GitReader(repoMock.Object, _defaultPatterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Changes().ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyCommitsWithoutTagOrVersionAreSkipped()
        {
            var today = DateTimeOffset.Now;
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            var twoDaysAgo = DateTimeOffset.Now.AddDays(-2);
            var changes = new List<IChange>
            {
                new GitChange("0.2.0", " ", "Without tag", today),
                new GitChange(" ", "Added", "WithoutVersion", yesterday),
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(changes));
            var reader = new GitReader(repoMock.Object, _defaultPatterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Changes().ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyOverriddenChangesAreUsed()
        {
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            var twoDaysAgo = DateTimeOffset.Now.AddDays(-2);
            var badChange = new GitChange(" ", "Added", "WithoutVersion", yesterday);
            var fixedChange = new GitChange("0.2.0", badChange.Tag, badChange.Summary, badChange.Date);
            var badChangeId = ToSha1String(badChange);
            var overrides = new Dictionary<string, IChange>
            {
                {badChangeId, fixedChange}
            };
            var changes = new List<IChange>
            {
                badChange,
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var expectedChanges = new List<IChange>
            {
                fixedChange,
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(changes));
            var reader = new GitReader(repoMock.Object, _defaultPatterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Changes(overrides).ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyOverriddenChangeVersionsAreUsedForPrecedingUnreleasedVersions()
        {
            var overriddenChange = new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.Date);
            var overrideChange = new GitChange(overriddenChange.Version, overriddenChange.Tag, "Some other summary", overriddenChange.Date);
            var unreleasedChange = new GitChange("Unreleased", overriddenChange.Tag, overriddenChange.Summary, overriddenChange.Date);
            var badChangeId = ToSha1String(overriddenChange);
            var idToOverrideChange = new Dictionary<string, IChange>
            {
                {badChangeId, overrideChange}
            };
            var repositoryChanges = new List<IChange>
            {
                overriddenChange,
                unreleasedChange
            };
            var expectedChanges = new List<IChange>
            {
                overrideChange,
                new GitChange(overrideChange.Version, unreleasedChange.Tag, unreleasedChange.Summary, unreleasedChange.Date)
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(repositoryChanges));
            var reader = new GitReader(repoMock.Object, _defaultPatterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Changes(idToOverrideChange).ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        private static IEnumerator<Commit> MockCommitEnumerator(IEnumerable<IChange> expectedChanges)
        {
            return expectedChanges.Select(MockCommit).GetEnumerator();
        }

        private static Commit MockCommit(IChange change)
        {
            ObjectId.TryParse(ToSha1String(change), out var id);
            var commitMock = new Mock<Commit>();
            var commitAuthor = new Signature("Some Author", "Some Email", change.Date);
            var expectedMessage = $@"{change.Summary}

reference: {change.Reference}
version: {change.Version}
tag: {change.Tag}
";
            commitMock.SetupGet(x => x.MessageShort).Returns(change.Summary);
            commitMock.SetupGet(x => x.Author).Returns(commitAuthor);
            commitMock.SetupGet(x => x.Message).Returns(expectedMessage);
            commitMock.SetupGet(x => x.Id).Returns(id);

            return commitMock.Object;
        }

        private static string ToSha1String(IChange change)
        {
            using (var sha1 = new SHA1Managed())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(change.ToString())).Select(b => b.ToString("x2")));
            }
        }
        
    }
}