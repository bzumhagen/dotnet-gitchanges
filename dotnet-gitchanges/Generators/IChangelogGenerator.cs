using System.Collections.Generic;

namespace Gitchanges.Generators
{
    public interface IChangelogGenerator<out T>
    {
        T Generate(string minVersion, IEnumerable<string> tagsToExclude);
    }
}