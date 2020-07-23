using System.Collections.Generic;
using Gitchanges.Changes;

namespace Gitchanges.Generators
{
    public interface IChangelogGenerator<out T>
    {
        T Generate(ChangeVersion minVersion, IEnumerable<string> changeTypesToExclude);
    }
}