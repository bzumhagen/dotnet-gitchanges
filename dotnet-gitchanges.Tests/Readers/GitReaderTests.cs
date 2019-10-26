using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;

namespace dotnet_gitchanges.Tests.Readers
{
    [TestFixture]
    public class GitReaderTests
    {
        [Test]
        public void VerifyCacheIsLoadedFromRepository()
        {
            var expectedChange = new GitChange("0.1.0", "Added", "Some Summary", DateTimeOffset.Now);
            var repoMock = new Mock<IRepository>();
            var commitLog = Mock.Of<IQueryableCommitLog>(cl => cl.GetEnumerator() == FakeCommitLog(expectedChange));
            var cacheMock = new Mock<IChangeCache>();
            var reader = new GitReader(repoMock.Object, cacheMock.Object);

            repoMock.Setup(r => r.Commits).Returns(commitLog);
            cacheMock.Setup(c => c.Add(expectedChange));
            
            reader.LoadCache();
            
            repoMock.VerifyAll();
            cacheMock.VerifyAll();
        }
        
        private static IEnumerator<Commit> FakeCommitLog(IChange expectedChange)
        {
            for (int i = 0; i < 1; i++)
            {
                yield return FakeCommit(expectedChange.Summary, expectedChange.Date);;
            }
        }

        private static Commit FakeCommit(string messageShort, DateTimeOffset when)
        {
            var commitMock = new Mock<Commit>();
            var commitAuthor = new Signature("Some Author", "Some Email", when);
            commitMock.SetupGet(x => x.MessageShort).Returns(messageShort);
            commitMock.SetupGet(x => x.Author).Returns(commitAuthor);

            return commitMock.Object;
        }
        
    }
}