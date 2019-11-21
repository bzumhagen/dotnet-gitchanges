using System;
using System.Text.RegularExpressions;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using LibGit2Sharp;

namespace Gitchanges.Readers.Parsers
{
    public class ProjectCommitParser : ICommitParser<ProjectChange>
    {
        private const string Unreleased = "Unreleased";
        private readonly ParsingPatterns _patterns;
        private string _lastVersion = Unreleased;
        
        public ProjectCommitParser(ParsingPatterns patterns)
        {
            _patterns = patterns;
        }
        
        public ProjectChange Parse(Commit commit)
        {
            var reference = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Reference));
            var version = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Version));
            var tag = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Tag));
            var project = GetMatchOrDefault(Regex.Match(commit.Message, _patterns.Project));
                    
            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(project)) return null;

            version = HandleVersion(version);
            
            return new ProjectChange(project, version, tag, commit.MessageShort, commit.Author.When, reference);
        }

        public ProjectChange Parse(ProjectChange overrideObject)
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