using System;
using System.IO;
using Gitchanges.Changes;
using Gitchanges.Readers.Parsers;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers.Parsers
{
    [TestFixture]
    public class OverrideProjectSourceRowParserTests
    {
        [Test]
        public void VerifyParserParsesLineWithReferenceSuccessfully()
        {
            var expectedChange = new OverrideProjectChange("MyProj", "123456", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date, "Some Reference");
            var line = $"{expectedChange.Id}|{expectedChange.Project}|{expectedChange.Reference}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new OverrideProjectSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParserParsesLineWithoutReferenceSuccessfully()
        {
            var expectedChange = new OverrideProjectChange("MyProj", "123456", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Id}|{expectedChange.Project}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new OverrideProjectSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.EqualTo(expectedChange));
        }
        
        [Test]
        public void VerifyParserHandlesTooFewDelimitersIssues()
        {
            var expectedChange = new OverrideProjectChange("MyProj", "123456", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Id}|{expectedChange.Project}|{expectedChange.Version}{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new OverrideProjectSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Wrong number of values. Expected 6 or 7 but was 5"));
        }
        
        [Test]
        public void VerifyParserHandlesTooManyDelimitersIssues()
        {
            var expectedChange = new OverrideProjectChange("MyProj", "123456", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Id}|{expectedChange.Project}|||{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date:yyyy-MM-dd}";
            var writer = new StringWriter();
            var parser = new OverrideProjectSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Wrong number of values. Expected 6 or 7 but was 8"));
        }
        
        [Test]
        public void VerifyParserHandlesDateFormatIssues()
        {
            var expectedChange = new OverrideProjectChange("MyProj", "123456", "0.1.0", "Some Tag", "Some Summary", DateTimeOffset.Now.Date);
            var line = $"{expectedChange.Id}|{expectedChange.Project}|{expectedChange.Version}|{expectedChange.Tag}|{expectedChange.Summary}|{expectedChange.Date}";
            var writer = new StringWriter();
            var parser = new OverrideProjectSourceRowParser(writer);
            var actual = parser.Parse(line);
            
            Assert.That(actual, Is.Null);
            Assert.That(writer.ToString().Trim(), Is.EqualTo($"Error parsing line '{line}'. Date should match the format 'yyyy-MM-dd'"));
        }
    }
}