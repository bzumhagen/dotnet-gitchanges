using System;
using System.IO;
using Gitchanges.Changes;
using Gitchanges.Readers.Parsers;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers.Parsers
{
    [TestFixture]
    public class ProjectFileSourceRowParserTests
    {
        [Test]
        public void VerifyParserParsesLineWithReferenceSuccessfully()
        {
            var expectedChange = new ProjectChange("MyProj", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date, "Some Reference");
            var line = $"{expectedChange.Project}|{expectedChange.Reference}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new ProjectFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParserParsesLineWithoutReferenceSuccessfully()
        {
            var expectedChange = new ProjectChange("MyProj","0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Project}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new ProjectFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParserHandlesTooFewDelimitersIssues()
        {
            var expectedChange = new ProjectChange("MyProj", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Project}|{expectedChange.Version}{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new ProjectFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Wrong number of values. Expected 5 or 6 but was 4"));
        }

        [Test]
        public void VerifyParserHandlesTooManyDelimiterIssues()
        {
            var expectedChange = new ProjectChange("MyProj", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Project}|{expectedChange.Version}|||{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new ProjectFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Wrong number of values. Expected 5 or 6 but was 7"));
        }
        
        [Test]
        public void VerifyParserHandlesDateFormatIssues()
        {
            var expectedChange = new ProjectChange("MyProj", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Project}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date}";
            var writer = new StringWriter();
            var parser = new ProjectFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Date should match the format 'yyyy-MM-dd'"));
        }
    }
}