namespace Gitchanges.Readers.Parsers
{
    public interface IRowParser<out T>
    {
        T Parse(string line);
    }
}