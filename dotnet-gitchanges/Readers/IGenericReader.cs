using System.Collections.Generic;

namespace Gitchanges.Readers
{
    public interface IGenericReader<out T>
    {
        IEnumerable<T> Values();
    }
}