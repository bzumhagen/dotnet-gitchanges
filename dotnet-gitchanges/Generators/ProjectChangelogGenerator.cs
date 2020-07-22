using System.Collections.Generic;
using System.Linq;
using Gitchanges.Caches;
using Gitchanges.Changes;
using Gitchanges.Enumerables;
using Gitchanges.Readers;
using Stubble.Core.Interfaces;

namespace Gitchanges.Generators
{
    
    public class ProjectChangelogGenerator : IChangelogGenerator<Dictionary<string, string>>
    {
        private readonly IEnumerable<IGenericReader<ProjectChange>> _readers;
        private readonly string _template;
        private readonly IStubbleRenderer _renderer;
        
        public ProjectChangelogGenerator(IEnumerable<IGenericReader<ProjectChange>> readers, string template, IStubbleRenderer renderer)
        {
            _readers = readers;
            _template = template;
            _renderer = renderer;
        }
        
        public Dictionary<string, string> Generate(string minVersion = null, IEnumerable<string> changeTypesToExclude = null)
        {
            var projectToCache = new Dictionary<string, IChangeCache>();
            var projectToOutput = new Dictionary<string, string>();
            var toExclude = changeTypesToExclude?.ToList() ?? new List<string>();
            foreach(var reader in _readers)
            {
                var values = reader.Values();
                if (minVersion != null || toExclude.Any())
                {
                    values = new FilteredChanges<ProjectChange>(values, minVersion, toExclude);
                }

                foreach (var lookup in values.ToLookup(change => change.Project))
                {
                    if (projectToCache.TryGetValue(lookup.Key, out var cache))
                    {
                        cache.Add(lookup.ToList());
                    }
                    else
                    {
                        cache = new ChangeCache();
                        cache.Add(lookup.ToList());
                        projectToCache.Add(lookup.Key, cache);
                    }
                }
            }

            foreach (var (project, cache) in projectToCache)
            {
                var output = _renderer.Render(_template, cache.GetAsValueDictionary());
                projectToOutput.Add(project, output);
            }
            return projectToOutput;
        }
    }
}