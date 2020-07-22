using System;
using System.Collections.Generic;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using Gitchanges.Readers.Parsers;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;
using static Gitchanges.Configuration.ParseableSourceType;

namespace Gitchanges.Tests.Readers.Parsers
{
    [TestFixture]
    public class DefaultCommitParserTests
    {

        private readonly ParsingConfig  _defaultParsingConfig = new ParsingConfig
        {
            Reference = new ParseableProperty{ SourceType = Message, Pattern= "reference:(.*)[\n]?", IsOptional = true },
            Version = new ParseableProperty{ SourceType = Message, Pattern= "version:(.*)[\n]?", IsOptional = false },
            ChangeType = new ParseableProperty{ SourceType = Message, Pattern= "type:(.*)[\n]?", IsOptional = false }
        };

        [Test]
        public void VerifyParsesWithGitTags()
        {
            var gitTags = new Dictionary<string, string>();
            gitTags.Add("THISCOULDBEASHA", "0.0.0");
            var expectedChange = new DefaultChange("0.0.0", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new DefaultCommitParser(_defaultParsingConfig, gitTags);

            var actual = parser.Parse(MockCommit(expectedChange));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }

        [Test]
        public void VerifyParsesWithGitTagsNoTagMatch()
        {
            var gitTags = new Dictionary<string, string>();
            gitTags.Add("THISCOULDBEASHA_OTHER", "0.0.0");
            var expectedChange = new DefaultChange("Unreleased", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new DefaultCommitParser(_defaultParsingConfig, gitTags);

            var actual = parser.Parse(MockCommit(expectedChange));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }

        [Test]
        public void VerifyParsesWithoutReference()
        {
            var expectedChange = new DefaultChange("Unreleased", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new DefaultCommitParser(_defaultParsingConfig);

            var actual = parser.Parse(MockCommit(expectedChange));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParsesWithReference()
        {
            var expectedChange = new DefaultChange("Unreleased", "Added", "Some Summary", DateTimeOffset.Now, "REF-1234");
            var parser = new DefaultCommitParser(_defaultParsingConfig);

            var actual = parser.Parse(MockCommit(expectedChange));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyUnreleasedCommitsHaveUnreleasedVersion()
        {
            var expectedChange = new DefaultChange("Unreleased", "Added", "Some Summary", DateTimeOffset.Now);
            var parser = new DefaultCommitParser(_defaultParsingConfig);

            var actual = parser.Parse(MockCommit(expectedChange));
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyReleasedCommitsWithUnreleasedInVersionHaveCorrectVersion()
        {
            var releasedChange = new DefaultChange("2.0.0", "Added", "Some Released Summary", DateTimeOffset.Now);
            var unreleasedChange = new DefaultChange("Unreleased", "Added", "Some Unreleased Summary", DateTimeOffset.Now);
            var parser = new DefaultCommitParser(_defaultParsingConfig);

            parser.Parse(MockCommit(releasedChange));
            var actual = parser.Parse(MockCommit(unreleasedChange));
            
            Assert.That(actual.Version, Is.EqualTo(releasedChange.Version));
            Assert.That(actual.ChangeType, Is.EqualTo(unreleasedChange.ChangeType));
            Assert.That(actual.Summary, Is.EqualTo(unreleasedChange.Summary));
            Assert.That(actual.Date, Is.EqualTo(unreleasedChange.Date));
            Assert.That(actual.Reference, Is.EqualTo(unreleasedChange.Reference));
        }
        
        [Test]
        public void VerifyCommitsWithUnreleasedInVersionHaveCorrectVersionWhenPrecededByExplicitChange()
        {
            var explicitChange = new DefaultChange("2.0.0", "Added", "Some Released Summary", DateTimeOffset.Now);
            var unreleasedChange = new DefaultChange("Unreleased", "Added", "Some Unreleased Summary", DateTimeOffset.Now);
            var parser = new DefaultCommitParser(_defaultParsingConfig);

            parser.Parse(explicitChange);
            var actual = parser.Parse(MockCommit(unreleasedChange));
            
            Assert.That(actual.Version, Is.EqualTo(explicitChange.Version));
            Assert.That(actual.ChangeType, Is.EqualTo(unreleasedChange.ChangeType));
            Assert.That(actual.Summary, Is.EqualTo(unreleasedChange.Summary));
            Assert.That(actual.Date, Is.EqualTo(unreleasedChange.Date));
            Assert.That(actual.Reference, Is.EqualTo(unreleasedChange.Reference));
        }
        
        private static Commit MockCommit(IChange change)
        {
            var commitMock = new Mock<Commit>();
            var commitAuthor = new Signature("Some Author", "Some Email", change.Date);
            var expectedMessage = $@"{change.Summary}

reference: {change.Reference}
version: {change.Version}
type: {change.ChangeType}
";
            commitMock.SetupGet(x => x.Sha).Returns("THISCOULDBEASHA");
            commitMock.SetupGet(x => x.MessageShort).Returns(change.Summary);
            commitMock.SetupGet(x => x.Author).Returns(commitAuthor);
            commitMock.SetupGet(x => x.Message).Returns(expectedMessage);

            return commitMock.Object;
        }
    }
}