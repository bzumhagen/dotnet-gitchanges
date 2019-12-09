using System;
using System.Collections.Generic;
using System.Linq;
using Gitchanges.Caches;
using Gitchanges.Changes;
using Gitchanges.Generators;
using Gitchanges.Readers;
using Moq;
using NUnit.Framework;
using Stubble.Core.Builders;

namespace Gitchanges.Tests.Generators
{
    [TestFixture]
    public class StringChangelogGeneratorTests
    {
        private const string Template = @"
{{#versions}}
## [{{version}}] - {{date}}
{{#tags}}
### {{tag}}
{{#changes}}
- {{#reference}}[{{{reference}}}] {{/reference}}{{summary}}
{{/changes}}

{{/tags}}
{{/versions}}";
        
        [Test]
        public void VerifyDefaultChangelogIsGenerated()
        {
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var readerMock = new Mock<IGenericReader<IChange>>();
            var cacheMock = new Mock<IChangeCache>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<IChange>>
            {
                readerMock.Object
            };
            var expectedChanges = new List<IChange>
            {
                new DefaultChange("1.0.0", "Added", "Added some other change", now),
                new DefaultChange("1.0.0", "Removed", "Removed some other change", now),
                new DefaultChange("0.1.0", "Added", "Added some change", yesterday),
                new DefaultChange("0.1.0", "Removed", "Removed some change", yesterday),
            };
            var expectedValueDictionary = ToValueDictionary(expectedChanges);
            readerMock.Setup(r => r.Values()).Returns(expectedChanges);
            cacheMock.Setup(c => c.Add(It.Is<IEnumerable<IChange>>(changes => changes.SequenceEqual(expectedChanges))));
            cacheMock.Setup(c => c.GetAsValueDictionary()).Returns(expectedValueDictionary);

            var expected = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other change

### Removed
- Removed some other change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some change

### Removed
- Removed some change

";
                    
            var generator = new StringChangelogGenerator(readers, cacheMock.Object, Template, renderer);
            var actual = generator.Generate(); 
            
            Assert.That(actual, Is.EqualTo(expected));
        }
        
        [Test]
        public void VerifyMinVersionChangelogIsGenerated()
        {
            var minVersion = "1.0.0";
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var readerMock = new Mock<IGenericReader<IChange>>();
            var cacheMock = new Mock<IChangeCache>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<IChange>>
            {
                readerMock.Object
            };
            var expectedChanges = new List<IChange>
            {
                new DefaultChange("1.0.0", "Added", "Added some other change", now),
                new DefaultChange("1.0.0", "Removed", "Removed some other change", now)
            };
            var allChanges = new List<IChange>(expectedChanges)
            {
                new DefaultChange("0.1.0", "Added", "Added some change", yesterday),
                new DefaultChange("0.1.0", "Removed", "Removed some change", yesterday)
            };
            var expectedValueDictionary = ToValueDictionary(expectedChanges);
            readerMock.Setup(r => r.Values()).Returns(allChanges);
            cacheMock.Setup(c => c.Add(It.Is<IEnumerable<IChange>>(changes => changes.SequenceEqual(expectedChanges))));
            cacheMock.Setup(c => c.GetAsValueDictionary()).Returns(expectedValueDictionary);

            var expected = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other change

### Removed
- Removed some other change

";
                    
            var generator = new StringChangelogGenerator(readers, cacheMock.Object, Template, renderer);
            var actual = generator.Generate(minVersion); 
            
            Assert.That(actual, Is.EqualTo(expected));
        }
        
        [Test]
        public void VerifyChangelogWithExclusionsIsGenerated()
        {
            var toExclude = new List<string>{"Removed"};
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var readerMock = new Mock<IGenericReader<IChange>>();
            var cacheMock = new Mock<IChangeCache>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<IChange>>
            {
                readerMock.Object
            };
            var expectedChanges = new List<IChange>
            {
                new DefaultChange("1.0.0", "Added", "Added some other change", now),
                new DefaultChange("0.1.0", "Added", "Added some change", yesterday),
            };
            var allChanges = new List<IChange>(expectedChanges)
            {
                new DefaultChange("1.0.0", "Removed", "Removed some other change", now),
                new DefaultChange("0.1.0", "Removed", "Removed some change", yesterday)
            };
            var expectedValueDictionary = ToValueDictionary(expectedChanges);
            readerMock.Setup(r => r.Values()).Returns(allChanges);
            cacheMock.Setup(c => c.Add(It.Is<IEnumerable<IChange>>(changes => changes.SequenceEqual(expectedChanges))));
            cacheMock.Setup(c => c.GetAsValueDictionary()).Returns(expectedValueDictionary);

            var expected = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some change

";
                    
            var generator = new StringChangelogGenerator(readers, cacheMock.Object, Template, renderer);
            var actual = generator.Generate(tagsToExclude: toExclude); 
            
            Assert.That(actual, Is.EqualTo(expected));
        }
        
        [Test]
        public void VerifyChangelogWithMultipleReadersIsGenerated()
        {
            var toExclude = new List<string>{"Removed"};
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var reader1Mock = new Mock<IGenericReader<IChange>>();
            var reader2Mock = new Mock<IGenericReader<IChange>>();
            var cacheMock = new Mock<IChangeCache>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<IChange>>
            {
                reader1Mock.Object,
                reader2Mock.Object,
            };
            var readerChanges1 = new List<IChange>
            {
                new DefaultChange("1.0.0", "Added", "Added some other change", now),
                new DefaultChange("0.1.0", "Added", "Added some change", yesterday),
            };
            var readerChanges2 = new List<IChange>
            {
                new DefaultChange("1.0.0", "Removed", "Removed some other change", now),
                new DefaultChange("0.1.0", "Removed", "Removed some change", yesterday)
            };
            var expectedChanges = readerChanges1.Concat(readerChanges2).ToList();
            var expectedValueDictionary = ToValueDictionary(expectedChanges);
            
            reader1Mock.Setup(r => r.Values()).Returns(readerChanges1);
            reader2Mock.Setup(r => r.Values()).Returns(readerChanges2);
            cacheMock.Setup(c => c.Add(It.Is<IEnumerable<IChange>>(changes => changes.SequenceEqual(expectedChanges))));
            cacheMock.Setup(c => c.GetAsValueDictionary()).Returns(expectedValueDictionary);

            var expected = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other change

### Removed
- Removed some other change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some change

### Removed
- Removed some change

";
                    
            var generator = new StringChangelogGenerator(readers, cacheMock.Object, Template, renderer);
            var actual = generator.Generate(tagsToExclude: toExclude); 
            
            Assert.That(actual, Is.EqualTo(expected));
        }
        
        private static Dictionary<string, object> ToValueDictionary(IEnumerable<IChange> changes)
        {
            var retVal = new Dictionary<string, object>();
            var versionList = new List<Dictionary<string, object>>();
            var versionLookup = changes.ToLookup(c => c.Version);
            foreach (var versionItem in versionLookup.OrderByDescending(v => v.Key))
            {
                var version = versionItem.Key;
                var tagLookup = versionItem.ToLookup(c => c.Tag);
                var versionDate = DateTimeOffset.MinValue;
                var tagsList = new List<Dictionary<string, object>>();
                foreach (var tagItem in tagLookup.OrderBy(t => t.Key))
                {
                    var tag = tagItem.Key;
                    var changesList = new List<Dictionary<string, object>>();
                    
                    foreach (var change in tagItem.OrderByDescending(c => c.Date))
                    {
                        if (change.Date > versionDate) versionDate = change.Date;
                        
                        changesList.Add(new Dictionary<string, object>()
                        {
                            {"summary", change.Summary},
                            {"reference", change.Reference}
                        });
                    }
                    tagsList.Add(new Dictionary<string, object>()
                    {
                        {"tag", tag},
                        {"changes", changesList}
                    });
                }
                versionList.Add(new Dictionary<string, object>()
                {
                    {"version", version},
                    {"date", versionDate.ToString("yyyy-MM-dd")},
                    {"tags", tagsList}
                });
            }
            retVal.Add("versions", versionList);
            return retVal;
        }
    }
}