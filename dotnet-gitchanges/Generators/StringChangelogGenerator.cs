using System.Collections.Generic;
using System.Linq;
using Gitchanges.Caches;
using Gitchanges.Changes;
using Gitchanges.Enumerables;
using Gitchanges.Readers;
using Stubble.Core.Interfaces;

namespace Gitchanges.Generators
{
    
    public class StringChangelogGenerator : IChangelogGenerator<string>
    {
        private readonly IEnumerable<IGenericReader<IChange>> _readers;
        private readonly IChangeCache _cache;
        private readonly string _template;
        private readonly IStubbleRenderer _renderer;
        
        public StringChangelogGenerator(IEnumerable<IGenericReader<IChange>> readers, IChangeCache cache, string template, IStubbleRenderer renderer)
        {
            _readers = readers;
            _cache = cache;
            _template = template;
            _renderer = renderer;
        }
        
        public string Generate(ChangeVersion minVersion = null, IEnumerable<string> changeTypesToExclude = null)
        {
            var toExclude = changeTypesToExclude?.ToList() ?? new List<string>();
            foreach(var reader in _readers)
            {
                var values = reader.Values();
                if (minVersion != null || toExclude.Any())
                {
                    values = new FilteredChanges<IChange>(values, minVersion, toExclude);
                }

                _cache.Add(values);
            }

            var results = _cache.GetAsValueDictionary();
            return _renderer.Render(_template, results);
        }
    }
}