using System;
using System.Text.RegularExpressions;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using LibGit2Sharp;

namespace Gitchanges.Readers.Parsers
{
    public class DefaultCommitParser : ICommitParser<IChange>
    {
        private const string Unreleased = "Unreleased";
        private readonly ParsingPatterns _patterns;
        private string _lastVersion = Unreleased;
        
        public DefaultCommitParser(ParsingPatterns patterns)
        {
            _patterns = patterns;
        }
        
        public IChange Parse(Commit commit)
        {
            var reference = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Reference));
            var version = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Version));
            var tag = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Tag));
                    
            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(tag)) return null;

            version = HandleVersion(version);
            
            return new DefaultChange(version, tag, commit.MessageShort, commit.Author.When, reference);
        }

        public IChange Parse(IChange overrideObject)
        {
            HandleVersion(overrideObject.Version);
            return overrideObject;
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