using LibGit2Sharp;

namespace Gitchanges.Readers.Parsers
{
    public interface ICommitParser<T>
    {
        T Parse(Commit commit);
        T Parse(T overrideObject);
    }
}