using System;
using System.IO;
using Gitchanges.Changes;
using Gitchanges.Readers.Parsers;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers.Parsers
{
    [TestFixture]
    public class DefaultFileSourceRowParserTests
    {
        [Test]
        public void VerifyParserParsesLineWithReferenceSuccessfully()
        {
            var expectedChange = new DefaultChange("0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date, "Some Reference");
            var line = $"{expectedChange.Reference}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new DefaultFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParserParsesLineWithoutReferenceSuccessfully()
        {
            var expectedChange = new DefaultChange("0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new DefaultFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParserHandlesTooFewDelimitersIssues()
        {
            var expectedChange = new DefaultChange("0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Version}{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new DefaultFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Wrong number of values. Expected 4 or 5 but was 3"));
        }

        [Test]
        public void VerifyParserHandlesTooManyDelimiterIssues()
        {
            var expectedChange = new DefaultChange("0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Version}|||{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new DefaultFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Wrong number of values. Expected 4 or 5 but was 6"));
        }
        
        [Test]
        public void VerifyParserHandlesDateFormatIssues()
        {
            var expectedChange = new DefaultChange("0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date}";
            var writer = new StringWriter();
            var parser = new DefaultFileSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Date should match the format 'yyyy-MM-dd'"));
        }
    }
}