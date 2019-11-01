using System.Collections.Generic;

namespace dotnet_gitchanges
{
    public interface IRepositoryReader
    {
        IEnumerable<IChange> Changes();
    }
}