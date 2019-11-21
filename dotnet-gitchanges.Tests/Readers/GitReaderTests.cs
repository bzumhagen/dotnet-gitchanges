using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Gitchanges.Changes;
using Gitchanges.Readers;
using Gitchanges.Readers.Parsers;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers
{
    [TestFixture]
    public class GitReaderTests
    {
        [Test]
        public void VerifyReaderReadsFromRepository()
        {
            var expectedChanges = new List<IChange>
            {
                new DefaultChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now),
                new DefaultChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1))
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(expectedChanges));
            var parserMock = new Mock<ICommitParser<IChange>>();
            var reader = new GitReader<IChange>(repoMock.Object, parserMock.Object);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            foreach (var expectedChange in expectedChanges)
            {
                parserMock.Setup(p => p.Parse(It.Is<Commit>(c => c.Id.Sha == ToSha1String(expectedChange)))).Returns(expectedChange);
            }

            Assert.That(reader.Values(), Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyUnparseableAreSkipped()
        {
            var today = DateTimeOffset.Now;
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            var twoDaysAgo = DateTimeOffset.Now.AddDays(-2);
            var changes = new List<IChange>
            {
                new DefaultChange("0.2.0", " ", "Without tag", today),
                new DefaultChange(" ", "Added", "WithoutVersion", yesterday),
                new DefaultChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var expectedChanges = new List<IChange>
            {
                new DefaultChange("0.1.0", "Removed", "Another Summary", twoDaysAgo)
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(changes));
            var parserMock = new Mock<ICommitParser<IChange>>();
            var reader = new GitReader<IChange>(repoMock.Object, parserMock.Object);

            parserMock.SetupSequence(p => p.Parse(It.IsAny<Commit>()))
                .Returns((IChange)null)
                .Returns((IChange)null)
                .Returns(expectedChanges.First);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            var actualChanges = reader.Values().ToList();
            Assert.That(actualChanges, Is.EquivalentTo(expectedChanges));
            repoMock.VerifyAll();
        }
        
        [Test]
        public void VerifyOverriddenChangesAreUsed()
        {
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            var twoDaysAgo = DateTimeOffset.Now.AddDays(-2);
            var badChange = new DefaultChange(" ", "Added", "WithoutVersion", yesterday);
            var fixedChange = new DefaultChange("0.2.0", badChange.Tag, badChange.Summary, badChange.Date);
            var fineChange = new DefaultChange("0.1.0", "Removed", "Another Summary", twoDaysAgo);
            var overrides = new Dictionary<string, IChange>
            {
                {ToSha1String(badChange), fixedChange}
            };
            var changes = new List<IChange>
            {
                badChange,
                fineChange
            };
            var expectedChanges = new List<IChange>
            {
                fixedChange,
                fineChange
            };
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == MockCommitEnumerator(changes));
            var parserMock = new Mock<ICommitParser<IChange>>();
            parserMock.Setup(p => p.Parse(fixedChange)).Returns(fixedChange);
            parserMock.Setup(p => p.Parse(It.IsAny<Commit>())).Returns(fineChange);
            repoMock.Setup(r => r.Commits).Returns(commitLog);

            var reader = new GitReader<IChange>(repoMock.Object, parserMock.Object, overrides);
            var actualChanges = reader.Values();
            
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