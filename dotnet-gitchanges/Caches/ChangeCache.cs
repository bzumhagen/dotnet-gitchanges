using System;
using System.Collections.Generic;
using System.Linq;
using Gitchanges.Changes;

namespace Gitchanges.Caches
{
    public class ChangeCache : IChangeCache
    {
        public class ChangeKey
        {
            public ChangeVersion Version { get; }
            public string ChangeType { get; }

            public ChangeKey(IChange change)
            {
                Version = change.Version;
                ChangeType = change.ChangeType;
            }
            protected bool Equals(ChangeKey other)
            {
                return Version.Equals(other.Version) && ChangeType == other.ChangeType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ChangeKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Version != null ? Version.GetHashCode() : 0) * 397) ^ (ChangeType != null ? ChangeType.GetHashCode() : 0);
                }
            }
        }
            
        public Dictionary<ChangeKey, IList<IChange>> ChangeKeyToChanges { get; }

        public ChangeCache()
        {
            ChangeKeyToChanges = new Dictionary<ChangeKey, IList<IChange>>();
        }
        
        public Dictionary<string, object> GetAsValueDictionary()
        {
            var retVal = new Dictionary<string, object>();
            var versionList = new List<Dictionary<string, object>>();
            Dictionary<ChangeVersion, Dictionary<string, IEnumerable<IChange>>> versionGroups =
                ChangeKeyToChanges
                    .GroupBy(pair => pair.Key.Version)
                    .ToDictionary(group => group.Key, group => 
                        group
                            .GroupBy(pair => pair.Key.ChangeType)
                            .ToDictionary(innerGroup => innerGroup.Key, innerGroup => innerGroup.SelectMany(pair => pair.Value))
                    );
            foreach (var version in versionGroups.Keys.OrderByDescending(v => v))
            {
                var changeTypeToChanges = versionGroups[version];
                var versionDate = DateTimeOffset.MinValue;
                var changeTypeList = new List<Dictionary<string, object>>();
                foreach (var changeType in changeTypeToChanges.Keys.OrderBy(t => t))
                {
                    var changeTypeChanges = changeTypeToChanges[changeType];
                    var changesList = new List<Dictionary<string, object>>();
                    
                    foreach (var change in changeTypeChanges.OrderByDescending (c => c.Date))
                    {
                        if (change.Date > versionDate) versionDate = change.Date;
                        
                        changesList.Add(new Dictionary<string, object>()
                        {
                            {"summary", change.Summary},
                            {"reference", change.Reference}
                        });
                    }
                    changeTypeList.Add(new Dictionary<string, object>()
                    {
                        {"changeType", changeType},
                        {"changes", changesList}
                    });
                }
                versionList.Add(new Dictionary<string, object>()
                {
                    {"version", version},
                    {"date", versionDate.ToString("yyyy-MM-dd")},
                    {"changeTypes", changeTypeList}
                });
            }
            retVal.Add("versions", versionList);
            return retVal;
        }
        
        public void Add(IEnumerable<IChange> changes)
        {
            foreach (var change in changes)
            {
                var changeKey = new ChangeKey(change);
                if (ChangeKeyToChanges.TryGetValue(changeKey, out var existingChanges))
                {
                    existingChanges.Add(change);
                }
                else
                {
                    ChangeKeyToChanges.Add(changeKey, new List<IChange>{ change });
                }
            }
        }
    }
}