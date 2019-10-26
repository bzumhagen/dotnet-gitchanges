using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnet_gitchanges
{
    public class ChangeCache : IChangeCache
    {
        public class ChangeKey
        {
            public string Version { get; }
            public string Tag { get; }

            public ChangeKey(IChange change)
            {
                Version = change.Version;
                Tag = change.Tag;
            }
            protected bool Equals(ChangeKey other)
            {
                return Version == other.Version && Tag == other.Tag;
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
                    return ((Version != null ? Version.GetHashCode() : 0) * 397) ^ (Tag != null ? Tag.GetHashCode() : 0);
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
            Dictionary<string, Dictionary<string, IEnumerable<IChange>>> versionGroups =
                ChangeKeyToChanges
                    .GroupBy(pair => pair.Key.Version)
                    .ToDictionary(group => group.Key, group => 
                        group
                            .GroupBy(pair => pair.Key.Tag)
                            .ToDictionary(innerGroup => innerGroup.Key, innerGroup => innerGroup.SelectMany(pair => pair.Value))
                    );
            foreach (var version in versionGroups.Keys.OrderByDescending(v => v))
            {
                var tagToChanges = versionGroups[version];
                var versionDate = DateTimeOffset.MinValue;
                var tagsList = new List<Dictionary<string, object>>();
                foreach (var tag in tagToChanges.Keys.OrderBy(t => t))
                {
                    var tagChanges = tagToChanges[tag];
                    var changesList = new List<Dictionary<string, object>>();
                    
                    foreach (var change in tagChanges.OrderByDescending (c => c.Date))
                    {
                        if (change.Date > versionDate) versionDate = change.Date;
                        
                        changesList.Add(new Dictionary<string, object>()
                        {
                            {"summary", change.Summary}
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
        
        public void Add(IChange change)
        {
            var changeKey = new ChangeKey(change);
            if (ChangeKeyToChanges.TryGetValue(changeKey, out var changes))
            {
                changes.Add(change);
            }
            else
            {
                ChangeKeyToChanges.Add(changeKey, new List<IChange>{ change });
            }
        }
    }
}