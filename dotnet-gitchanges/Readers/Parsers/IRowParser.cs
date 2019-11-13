namespace Gitchanges.Readers.Parsers
{
    public interface IRowParser<T>
    {
        T Parse(string line);
    }
}