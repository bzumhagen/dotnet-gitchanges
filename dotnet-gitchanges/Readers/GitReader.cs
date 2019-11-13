using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using LibGit2Sharp;

namespace Gitchanges.Readers
{
    public class GitReader : IRepositoryReader
    {
        private const string Unreleased = "Unreleased";
        private readonly IRepository _repository;
        private readonly ParsingPatterns _patterns;
        private string _lastVersion = Unreleased;
        
        public GitReader(IRepository repository, ParsingPatterns patterns)
        {
            _repository = repository;
            _patterns = patterns;
        }

        public IEnumerable<IChange> Changes()
        {
            using (var repo = _repository)
            {
                foreach (var commit in repo.Commits.AsEnumerable())
                {
                    var change = Parse(commit);
                    if (change == null) continue;
                    yield return change;
                }
            }
        }
        
        public IEnumerable<IChange> Changes(Dictionary<string, IChange> overrides)
        {
            using (var repo = _repository)
            {
                foreach (var commit in repo.Commits.AsEnumerable())
                {
                    IChange change;
                    if (overrides.TryGetValue(commit.Id.ToString(), out var overrideChange))
                    {
                        var version = HandleVersion(overrideChange.Version);
                        change = new GitChange(version, overrideChange.Tag, overrideChange.Summary, overrideChange.Date, overrideChange.Reference);
                    }
                    else
                    {
                        change = Parse(commit);
                    }
                    
                    if (change == null) continue;
                    yield return change;
                }
            }
        }

        private IChange Parse(Commit commit)
        {
            var reference = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Reference));
            var version = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Version));
            var tag = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Tag));
                    
            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(tag)) return null;

            version = HandleVersion(version);
            
            return new GitChange(version, tag, commit.MessageShort, commit.Author.When, reference);
        }

        private static string GetMatchOrDefault(Match match)
        {
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private string HandleVersion(string version)
        {
            if (string.Equals(version, Unreleased, StringComparison.CurrentCultureIgnoreCase))
            {
                version = _lastVersion;
            }
            else
            {
                _lastVersion = version;
            }

            return version;
        }
        
    }
}