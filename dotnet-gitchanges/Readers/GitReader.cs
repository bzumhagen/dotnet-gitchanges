using System.Collections.Generic;
using System.Linq;
using Gitchanges.Changes;
using Gitchanges.Readers.Parsers;
using LibGit2Sharp;

namespace Gitchanges.Readers
{
    public class GitReader<T> : IGenericReader<T>
    {
        private readonly IRepository _repository;
        private readonly ICommitParser<T> _parser;
        private readonly Dictionary<string, T> _idToOverrideObject;
        
        public GitReader(IRepository repository, ICommitParser<T> parser, Dictionary<string, T> idToOverrideObject = null)
        {
            _repository = repository;
            _parser = parser;
            _idToOverrideObject = idToOverrideObject;
        }

        public IEnumerable<T> Values()
        {
            using (var repo = _repository)
            {
                if (_idToOverrideObject != null && _idToOverrideObject.Count > 0)
                {
                    foreach (var commit in repo.Commits.AsEnumerable())
                    {
                        var result = _idToOverrideObject.TryGetValue(commit.Id.ToString(), out var overrideObject) ? _parser.Parse(overrideObject) : _parser.Parse(commit);
                        if (result == null) continue;
                        yield return result;
                    }
                }
                else
                {
                    foreach (var commit in repo.Commits.AsEnumerable())
                    {
                        var change = _parser.Parse(commit);
                        if (change == null) continue;
                        yield return change;
                    }
                }
            }
        }
        
    }
}