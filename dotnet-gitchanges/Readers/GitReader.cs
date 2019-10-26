using System;
using System.Globalization;
using System.Linq;
using LibGit2Sharp;

namespace dotnet_gitchanges
{
    public class GitReader : IRepositoryReader
    {
        private readonly IRepository _repository;
        private readonly IChangeCache _cache;
        
        public GitReader(IRepository repository, IChangeCache cache)
        {
            _repository = repository;
            _cache = cache;
        }
        
        public void LoadCache()
        {
            using (var repo = _repository)
            {
                foreach (var commit in repo.Commits.AsEnumerable())
                {
                    var change = new GitChange("0.1.0", "Added", commit.MessageShort, commit.Author.When);
                    _cache.Add(change);
                }
            }
        }
    }
}