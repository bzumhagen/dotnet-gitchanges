using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using Gitchanges.Readers;
using LibGit2Sharp;
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
        public void VerifyReaderReadsFromFileWithoutReference()
        {
            var fileContents = new StringBuilder();
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.Date),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1).Date)
            };
            foreach (var change in expectedChanges)
            {
                fileContents.Append($"{change.Version}{Delimiter}{change.Tag}{Delimiter}{change.Summary}{Delimiter}{change.Date.ToString(DateFormat)}\n");
            }
            UsingTempFile(fileContents.ToString(), path =>
            {
                var reader = new FileReader(path, Delimiter, null);
            
                Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
            });
        }
        
        [Test]
        public void VerifyReaderReadsFromFileWithReference()
        {
            var fileContents = new StringBuilder();
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.2.0", "Added", "Some Summary", DateTimeOffset.Now.Date, "REF-1234"),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1).Date, "REF-5678")
            };
            foreach (var change in expectedChanges)
            {
                fileContents.Append($"{change.Reference}{Delimiter}{change.Version}{Delimiter}{change.Tag}{Delimiter}{change.Summary}{Delimiter}{change.Date.ToString(DateFormat)}\n");
            }
            UsingTempFile(fileContents.ToString(), path =>
            {
                var reader = new FileReader(path, Delimiter, null);
            
                Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
            });
        }
        
        [Test]
        public void VerifyReaderSkipsUnparseableRecords()
        {
            var errorWriter = new StringWriter();
            var fileContents = new StringBuilder();
            var allChanges = new List<IChange>
            {
                new GitChange("|0.2.0", "Added", "Some Summary", DateTimeOffset.Now.Date, "REF-1234"),
                new GitChange("0.1.5", "Added|", "Some Summary", DateTimeOffset.Now.Date, "REF-1234"),
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1).Date, "REF-5678")
            };
            var expectedChanges = new List<IChange>
            {
                new GitChange("0.1.0", "Removed", "Another Summary", DateTimeOffset.Now.AddDays(-1).Date, "REF-5678")
            };
            foreach (var change in allChanges)
            {
                fileContents.Append($"{change.Reference}{Delimiter}{change.Version}{Delimiter}{change.Tag}{Delimiter}{change.Summary}{Delimiter}{change.Date.ToString(DateFormat)}\n");
            }
            UsingTempFile(fileContents.ToString(), path =>
            {
                var reader = new FileReader(path, Delimiter, errorWriter);
            
                Assert.That(reader.Changes(), Is.EquivalentTo(expectedChanges));
                Assert.That(errorWriter.ToString().Split("\n").Length, Is.EqualTo(3));
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