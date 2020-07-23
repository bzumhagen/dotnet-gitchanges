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
        private readonly ChangeVersion _minVersion;
        private readonly HashSet<string> _changeTypesToExclude;
        
        public FilteredChanges(IEnumerable<T> changes, ChangeVersion minVersion, IEnumerable<string> changeTypesToExclude)
        {
            _changes = changes;
            _minVersion = minVersion;
            _changeTypesToExclude = changeTypesToExclude.Select(tag => tag.ToLower()).ToHashSet();
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var change in _changes)
            {
                if (_minVersion != null && change.Version.CompareTo(_minVersion) < 0) break;
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