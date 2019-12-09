using System;
using System.Collections.Generic;
using Gitchanges.Changes;
using Gitchanges.Generators;
using Gitchanges.Readers;
using Moq;
using NUnit.Framework;
using Stubble.Core.Builders;

namespace Gitchanges.Tests.Generators
{
    [TestFixture]
    public class ProjectChangelogGeneratorTests
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
            var readerMock = new Mock<IGenericReader<ProjectChange>>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<ProjectChange>>
            {
                readerMock.Object
            };
            const string projectA = "ProjectA";
            const string projectB = "ProjectB";
            var expectedChanges = new List<ProjectChange>
            {
                new ProjectChange(projectA, "1.0.0", "Added", $"Added some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Added", $"Added some other {projectB} change", now),
                new ProjectChange(projectA, "1.0.0", "Removed", $"Removed some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Removed", $"Removed some other {projectB} change", now),
                new ProjectChange(projectA, "0.1.0", "Added", $"Added some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Added", $"Added some {projectB} change", yesterday),
                new ProjectChange(projectA, "0.1.0", "Removed", $"Removed some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Removed", $"Removed some {projectB} change", yesterday),
            };
            readerMock.Setup(r => r.Values()).Returns(expectedChanges);

            var expectedA = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectA} change

### Removed
- Removed some other {projectA} change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some {projectA} change

### Removed
- Removed some {projectA} change

";
            var expectedB = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectB} change

### Removed
- Removed some other {projectB} change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some {projectB} change

### Removed
- Removed some {projectB} change

";

            var generator = new ProjectChangelogGenerator(readers, Template, renderer);
            var projectToActual = generator.Generate(); 
            
            Assert.That(projectToActual.GetValueOrDefault(projectA), Is.EqualTo(expectedA));
            Assert.That(projectToActual.GetValueOrDefault(projectB), Is.EqualTo(expectedB));
        }
        
        [Test]
        public void VerifyMinVersionChangelogIsGenerated()
        {
            var minVersion = "1.0.0";
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var readerMock = new Mock<IGenericReader<ProjectChange>>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<ProjectChange>>
            {
                readerMock.Object
            };
            const string projectA = "ProjectA";
            const string projectB = "ProjectB";
            var expectedChanges = new List<ProjectChange>
            {
                new ProjectChange(projectA, "1.0.0", "Added", $"Added some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Added", $"Added some other {projectB} change", now),
                new ProjectChange(projectA, "1.0.0", "Removed", $"Removed some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Removed", $"Removed some other {projectB} change", now),
                new ProjectChange(projectA, "0.1.0", "Added", $"Added some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Added", $"Added some {projectB} change", yesterday),
                new ProjectChange(projectA, "0.1.0", "Removed", $"Removed some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Removed", $"Removed some {projectB} change", yesterday),
            };
            readerMock.Setup(r => r.Values()).Returns(expectedChanges);

            var expectedA = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectA} change

### Removed
- Removed some other {projectA} change

";
            var expectedB = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectB} change

### Removed
- Removed some other {projectB} change

";

            var generator = new ProjectChangelogGenerator(readers, Template, renderer);
            var projectToActual = generator.Generate(minVersion: minVersion); 
            
            Assert.That(projectToActual.GetValueOrDefault(projectA), Is.EqualTo(expectedA));
            Assert.That(projectToActual.GetValueOrDefault(projectB), Is.EqualTo(expectedB));
        }
        
        [Test]
        public void VerifyChangelogWithExclusionsIsGenerated()
        {
            var tagsToExclude = new List<string> {"Removed"};
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var readerMock = new Mock<IGenericReader<ProjectChange>>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<ProjectChange>>
            {
                readerMock.Object
            };
            const string projectA = "ProjectA";
            const string projectB = "ProjectB";
            var expectedChanges = new List<ProjectChange>
            {
                new ProjectChange(projectA, "1.0.0", "Added", $"Added some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Added", $"Added some other {projectB} change", now),
                new ProjectChange(projectA, "1.0.0", "Removed", $"Removed some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Removed", $"Removed some other {projectB} change", now),
                new ProjectChange(projectA, "0.1.0", "Added", $"Added some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Added", $"Added some {projectB} change", yesterday),
                new ProjectChange(projectA, "0.1.0", "Removed", $"Removed some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Removed", $"Removed some {projectB} change", yesterday),
            };
            readerMock.Setup(r => r.Values()).Returns(expectedChanges);

            var expectedA = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectA} change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some {projectA} change

";
            var expectedB = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectB} change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some {projectB} change

";

            var generator = new ProjectChangelogGenerator(readers, Template, renderer);
            var projectToActual = generator.Generate(tagsToExclude: tagsToExclude); 
            
            Assert.That(projectToActual.GetValueOrDefault(projectA), Is.EqualTo(expectedA));
            Assert.That(projectToActual.GetValueOrDefault(projectB), Is.EqualTo(expectedB));
        }
        
        [Test]
        public void VerifyChangelogWithMultipleReadersIsGenerated()
        {
            var tagsToExclude = new List<string> {"Removed"};
            var now = DateTimeOffset.Now;
            var yesterday = now.AddDays(-1);
            var reader1Mock = new Mock<IGenericReader<ProjectChange>>();
            var reader2Mock = new Mock<IGenericReader<ProjectChange>>();
            var renderer = new StubbleBuilder().Build();
            var readers = new List<IGenericReader<ProjectChange>>
            {
                reader1Mock.Object,
                reader2Mock.Object
            };
            const string projectA = "ProjectA";
            const string projectB = "ProjectB";
            var expectedChanges1 = new List<ProjectChange>
            {
                new ProjectChange(projectA, "1.0.0", "Added", $"Added some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Added", $"Added some other {projectB} change", now),
                new ProjectChange(projectA, "1.0.0", "Removed", $"Removed some other {projectA} change", now),
                new ProjectChange(projectB, "1.0.0", "Removed", $"Removed some other {projectB} change", now)
            };
            var expectedChanges2 = new List<ProjectChange>
            {
                new ProjectChange(projectA, "0.1.0", "Added", $"Added some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Added", $"Added some {projectB} change", yesterday),
                new ProjectChange(projectA, "0.1.0", "Removed", $"Removed some {projectA} change", yesterday),
                new ProjectChange(projectB, "0.1.0", "Removed", $"Removed some {projectB} change", yesterday)
            };
            reader1Mock.Setup(r => r.Values()).Returns(expectedChanges1);
            reader2Mock.Setup(r => r.Values()).Returns(expectedChanges2);

            var expectedA = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectA} change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some {projectA} change

";
            var expectedB = $@"
## [1.0.0] - {now:yyyy-MM-dd}
### Added
- Added some other {projectB} change

## [0.1.0] - {yesterday:yyyy-MM-dd}
### Added
- Added some {projectB} change

";

            var generator = new ProjectChangelogGenerator(readers, Template, renderer);
            var projectToActual = generator.Generate(tagsToExclude: tagsToExclude); 
            
            Assert.That(projectToActual.GetValueOrDefault(projectA), Is.EqualTo(expectedA));
            Assert.That(projectToActual.GetValueOrDefault(projectB), Is.EqualTo(expectedB));
        }
    }
}