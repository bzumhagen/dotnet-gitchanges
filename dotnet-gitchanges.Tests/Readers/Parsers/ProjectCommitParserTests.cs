using System;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using Gitchanges.Readers.Parsers;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers.Parsers
{
    [TestFixture]
    public class ProjectCommitParserTests
    {

        private readonly ParsingPatterns  _defaultPatterns = new ParsingPatterns
        {
            Reference = "reference:(.*)[\n]?",
            Version = "version:(.*)[\n]?",
            Tag = "tag:(.*)[\n]?",
            Project = "project:(.*)[\n]?"
        };
        
        [Test]
        public void VerifyParsesWithProject()
        {
            var expectedChange = new ProjectChange("MyProj", "Unreleased", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new ProjectCommitParser(_defaultPatterns);

            var actual = parser.Parse(MockCommit(expectedChange, expectedChange.Project, expectedChange.Version));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParsesWithoutProject()
        {
            var expectedChange = new ProjectChange("Unused", "Unreleased", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new ProjectCommitParser(_defaultPatterns);

            var actual = parser.Parse(MockCommit(expectedChange, "", expectedChange.Version));
            Assert.That(actual, Is.Null);
        }
        
        [Test]
        public void VerifyPropagatesNullFromBase()
        {
            var expectedChange = new ProjectChange("MyProj", "Unreleased", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new ProjectCommitParser(_defaultPatterns);

            var actual = parser.Parse(MockCommit(expectedChange, expectedChange.Project, ""));
            Assert.That(actual, Is.Null);
        }
        
        private static Commit MockCommit(IChange change, string project = "", string version = "")
        {
            var commitMock = new Mock<Commit>();
            var commitAuthor = new Signature("Some Author", "Some Email", change.Date);
            var expectedMessage = $@"{change.Summary}

reference: {change.Reference}
version: {version}
tag: {change.Tag}
project: {project}
";
            commitMock.SetupGet(x => x.MessageShort).Returns(change.Summary);
            commitMock.SetupGet(x => x.Author).Returns(commitAuthor);
            commitMock.SetupGet(x => x.Message).Returns(expectedMessage);

            return commitMock.Object;
        }
    }
}