using System.Collections.Generic;
using Gitchanges.Changes;

namespace Gitchanges.Readers
{
    public interface IRepositoryReader
    {
        IEnumerable<IChange> Changes();
    }
}