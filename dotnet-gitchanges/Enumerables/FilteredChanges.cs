using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gitchanges.Changes;

namespace Gitchanges.Enumerables
{
    public class FilteredChanges<T> : IEnumerable<T> where T : IChange
    {
        private readonly IEnumerable<T> _changes;
        private readonly string _minVersion;
        private readonly HashSet<string> _changeTypesToExclude;
        
        public FilteredChanges(IEnumerable<T> changes, string minVersion, IEnumerable<string> changeTypesToExclude)
        {
            _changes = changes;
            _minVersion = minVersion;
            _changeTypesToExclude = changeTypesToExclude.Select(tag => tag.ToLower()).ToHashSet();
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var change in _changes)
            {
                if (!string.IsNullOrEmpty(_minVersion) && string.Compare(change.Version, _minVersion, StringComparison.Ordinal) < 0) break;
                if (_changeTypesToExclude.Contains(change.ChangeType.ToLower())) continue;
                
                yield return change;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}