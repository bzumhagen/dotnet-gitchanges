﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using dotnet_gitchanges.Configuration;
using LibGit2Sharp;

namespace dotnet_gitchanges
{
    public class GitReader : IRepositoryReader
    {
        private readonly IRepository _repository;
        private readonly ParsingPatterns _patterns;
        
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
                    var reference = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Reference));
                    var version = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Version));
                    var tag = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Tag));
                    
                    yield return new GitChange(version, tag, commit.MessageShort, commit.Author.When, reference);
                }
            }
        }

        private static string GetMatchOrDefault(Match match)
        {
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }
    }
}