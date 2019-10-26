using System.Collections.Generic;

namespace dotnet_gitchanges
{
    public interface IChangeCache
    {
        Dictionary<string, object> GetAsValueDictionary();
        void Add(IChange change);
    }
}