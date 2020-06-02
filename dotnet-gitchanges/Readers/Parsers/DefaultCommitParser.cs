using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using LibGit2Sharp;

namespace Gitchanges.Readers.Parsers
{
    public class DefaultCommitParser : ICommitParser<IChange>
    {
        private const string Unreleased = "Unreleased";
        private const string Uncategorized = "Uncategorized";
        private readonly ParsingPatterns _patterns;
        private string _lastVersion = Unreleased;
        private IDictionary<string, string> _commitShaToTagName;
        
        public DefaultCommitParser(ParsingPatterns patterns, IDictionary<string, string> commitShaToTagName = null)
        {
            _commitShaToTagName = commitShaToTagName;
            _patterns = patterns;
        }
        
        public IChange Parse(Commit commit)
        {
            var reference = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Reference));
            var tag = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Tag), _commitShaToTagName != null ? Uncategorized : "");
            string version = "";
            if (_commitShaToTagName != null)
            {
                version = Unreleased;
                if (_commitShaToTagName.ContainsKey(commit.Sha))
                {
                    version = _commitShaToTagName[commit.Sha];
                    _lastVersion = version;
                }
            }
            else
            {
                version = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Version));
                if (string.IsNullOrEmpty(version)) return null;
            }

            version = HandleVersion(version);

            return new DefaultChange(version, tag, commit.MessageShort, commit.Author.When, reference);
        }

        public IChange Parse(IChange overrideObject)
        {
            HandleVersion(overrideObject.Version);
            return overrideObject;
        }

        private static string GetMatchOrDefault(Match match, string def = "")
        {
            return match.Success ? match.Groups[1].Value.Trim() : def;
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