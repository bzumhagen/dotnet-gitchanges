using System;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using Gitchanges.Readers.Parsers;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using static Gitchanges.Configuration.ParseableSourceType;

namespace Gitchanges.Tests.Readers.Parsers
{
    [TestFixture]
    public class ProjectCommitParserTests
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        
        private readonly ParsingConfig  _defaultParsingConfig = new ParsingConfig
        {
            Reference = new ParseableProperty{ SourceType = Message, Pattern= "reference:(.*)[\n]?", IsOptional = true },
            Version = new ParseableProperty{ SourceType = Message, Pattern= "version:(.*)[\n]?", IsOptional = false },
            ChangeType = new ParseableProperty{ SourceType = Message, Pattern= "type:(.*)[\n]?", IsOptional = false },
            Project = new ParseableProperty{ SourceType = Message, Pattern= "project:(.*)[\n]?", IsOptional = false }
        };
        
        [Test]
        public void VerifyParsesWithProject()
        {
            var expectedChange = new ProjectChange("MyProj", new ChangeVersion("Unreleased"), "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new ProjectCommitParser(_loggerFactory, _defaultParsingConfig);

            var actual = parser.Parse(MockCommit(expectedChange, expectedChange.Project, expectedChange.Version));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParsesWithoutProject()
        {
            var expectedChange = new ProjectChange("Unused", new ChangeVersion("Unreleased"), "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new ProjectCommitParser(_loggerFactory, _defaultParsingConfig);

            var actual = parser.Parse(MockCommit(expectedChange, "", expectedChange.Version));
            Assert.That(actual, Is.Null);
        }
        
        [Test]
        public void VerifyPropagatesNullFromBase()
        {
            var expectedChange = new ProjectChange("MyProj", new ChangeVersion("Unreleased"), "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new ProjectCommitParser(_loggerFactory, _defaultParsingConfig);

            var actual = parser.Parse(MockCommit(expectedChange, expectedChange.Project));
            Assert.That(actual, Is.Null);
        }
        
        private static Commit MockCommit(IChange change, string project = "", ChangeVersion version = null)
        {
            var commitMock = new Mock<Commit>();
            var commitAuthor = new Signature("Some Author", "Some Email", change.Date);
            var expectedMessage = $@"{change.Summary}

reference: {change.Reference}
version: {version}
type: {change.ChangeType}
project: {project}
";
            commitMock.SetupGet(x => x.MessageShort).Returns(change.Summary);
            commitMock.SetupGet(x => x.Author).Returns(commitAuthor);
            commitMock.SetupGet(x => x.Message).Returns(expectedMessage);

            return commitMock.Object;
        }
    }
}