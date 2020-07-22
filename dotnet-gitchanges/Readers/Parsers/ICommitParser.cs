using Gitchanges.Changes;
using LibGit2Sharp;

namespace Gitchanges.Readers.Parsers
{
    public interface ICommitParser
    {
	    IChange Parse(Commit commit);
	    IChange Parse(IChange overrideObject);
    }
}