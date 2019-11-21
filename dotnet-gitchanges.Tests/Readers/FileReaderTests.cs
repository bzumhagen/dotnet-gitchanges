using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitchanges.Changes;
using Gitchanges.Readers;
using Gitchanges.Readers.Parsers;
using Moq;
using NUnit.Framework;

namespace Gitchanges.Tests.Readers
{
    [TestFixture]
    public class FileReaderTests
    {
        private const char Delimiter = '|';
        private const string DateFormat = "yyyy-MM-dd";
        
        [Test]
        public void VerifyReaderReadsFromFile()
        {
            var fileContents = new StringBuilder();
            var expectedChanges = new List<IChange>
            {
                new DefaultChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.Date),
                new DefaultChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1).Date)
            };
            var mockParser = new Mock<IRowParser<IChange>>();
            foreach (var change in expectedChanges)
            {
                var line = $"{change.Version}{Delimiter}{change.Tag}{Delimiter}{change.Summary}{Delimiter}{change.Date.ToString(DateFormat)}\n";
                mockParser.Setup(p => p.Parse(line.Trim())).Returns(change);
                fileContents.Append(line);
            }
            UsingTempFile(fileContents.ToString(), path =>
            {
                var reader = new FileReader<IChange>(path, mockParser.Object);
            
                Assert.That(reader.Values(), Is.EquivalentTo(expectedChanges));
            });
        }
        
        [Test]
        public void VerifyReaderReadsFromFileSkippingUnparseableRecords()
        {
            var fileContents = new StringBuilder();
            var allChanges = new List<IChange>
            {
                new DefaultChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.Date),
                new DefaultChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1).Date)
            };
            var expectedChanges = new List<IChange>
            {
                allChanges.First()
            };
            var mockParser = new Mock<IRowParser<IChange>>();
            mockParser.SetupSequence(p => p.Parse(It.IsAny<string>())).Returns((IChange)null).Returns(expectedChanges.First());
            
            foreach (var change in allChanges)
            {
                var line = $"{change?.Version}{Delimiter}{change?.Tag}{Delimiter}{change?.Summary}{Delimiter}{change?.Date.ToString(DateFormat)}\n";
                fileContents.Append(line);
            }
            
            UsingTempFile(fileContents.ToString(), path =>
            {
                var reader = new FileReader<IChange>(path, mockParser.Object);
            
                Assert.That(reader.Values(), Is.EquivalentTo(expectedChanges));
            });
        }
        
        private static void UsingTempFile(string contents, Action<string> test)
        {
            string path = Path.GetTempFileName();

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.None))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(contents);
                };
            }

            try
            {
                test(path);
            }
            finally
            {
                File.Delete(path);
            }
        }
        
    }
}