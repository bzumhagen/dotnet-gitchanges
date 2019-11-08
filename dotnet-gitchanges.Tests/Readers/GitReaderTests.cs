using System;
using System.Collections.Generic;
using System.Linq;
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
        [Test]
        public void VerifyCacheIsLoadedFromRepository()
        {
            var patterns = new ParsingPatterns
            {
                Reference = "reference:(.*)[\n]?",
                Version = "version:(.*)[\n]?",
                Tag = "tag:(.*)[\n]?"
            };
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1))
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(expectedChanges));
            var reader = new GitReader(repoMock.Object, patterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            
            Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyUnreleasedCommitsHaveUnreleasedVersion()
        {
            var patterns = new ParsingPatterns
            {
                Reference = "reference:(.*)[\n]?",
                Version = "version:(.*)[\n]?",
                Tag = "tag:(.*)[\n]?"
            };
            var expectedChanges = new List<IChange>
            {
                new GitChange("Unreleased", "Added", "Some Unreleased Summary", DateTimeOffset.Now),
                new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.AddDays(-1)),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-2))
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(expectedChanges));
            var reader = new GitReader(repoMock.Object, patterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            
            Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyReleasedCommitsWithUnreleasedInVersionHaveCorrectVersion()
        {
            var patterns = new ParsingPatterns
            {
                Reference = "reference:(.*)[\n]?",
                Version = "version:(.*)[\n]?",
                Tag = "tag:(.*)[\n]?"
            };
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
            var reader = new GitReader(repoMock.Object, patterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Changes().ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyCommitsWithoutTagOrVersionAreSkipped()
        {
            var patterns = new ParsingPatterns
            {
                Reference = "reference:(.*)[\n]?",
                Version = "version:(.*)[\n]?",
                Tag = "tag:(.*)[\n]?"
            };
            var today = DateTimeOffset.Now;
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            var twoDaysAgo = DateTimeOffset.Now.AddDays(-2);
            var changes = new List<IChange>
            {
                new GitChange("0.2.0", "", "Without tag", today),
                new GitChange("", "Added", "WithoutVersion", yesterday),
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(changes));
            var reader = new GitReader(repoMock.Object, patterns);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Changes().ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        private static IEnumerator<Commit> MockCommitEnumerator(IEnumerable<IChange> expectedChanges)
        {
            foreach (var expectedChange in expectedChanges)
            {
                yield return MockCommit(expectedChange);
            }
        }

        private static Commit MockCommit(IChange change)
        {
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

            return commitMock.Object;
        }
        
    }
}