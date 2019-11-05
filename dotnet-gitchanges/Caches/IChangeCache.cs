using System.Collections.Generic;
using Gitchanges.Changes;

namespace Gitchanges.Caches
{
    public interface IChangeCache
    {
        Dictionary<string, object> GetAsValueDictionary();
        void Add(IChange change);
    }
}