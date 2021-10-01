using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gitchanges.Changes;
using Gitchanges.Configuration;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Gitchanges.Readers.Parsers
{
    public class DefaultCommitParser : ICommitParser
    {
        private const string Unreleased = "Unreleased";
        protected const string Uncategorized = "Uncategorized";
        protected readonly ParsingConfig ParsingConfig;
        protected string LastVersion = Unreleased;
        private readonly IDictionary<string, string> _commitShaToTagName;
        private readonly ILogger<DefaultCommitParser> _logger;
        
        public DefaultCommitParser(ILoggerFactory loggerFactory, ParsingConfig parsingConfig, IDictionary<string, string> commitShaToTagName = null)
        {
            _logger = loggerFactory.CreateLogger<DefaultCommitParser>();
            _commitShaToTagName = commitShaToTagName;
            ParsingConfig = parsingConfig;
        }
        
        public IChange Parse(Commit commit)
        {
            var reference = GetMatchOrDefault(ParsingConfig.Reference, commit);
            var changeType = GetMatchOrDefault(ParsingConfig.ChangeType, commit, Uncategorized);
            var version = GetMatchOrDefault(ParsingConfig.Version, commit, LastVersion);
            
            _logger.LogInformation($"commit #{commit.Id} - reference: {reference}, changeType: {changeType}, version: {version}");

            if (reference == null || changeType == null || version == null)
            {
                return null;
            }

            version = HandleVersion(version);
                
            return new DefaultChange(new ChangeVersion(version), changeType, commit.MessageShort, commit.Author.When, reference);
        }

        public IChange Parse(IChange overrideObject)
        {
            HandleVersion(overrideObject.Version.ToString());
            return overrideObject;
        }

        private string GetSource(Commit commit, ParseableSourceType sourceType)
        {
            return sourceType switch
            {
                ParseableSourceType.Message => commit.Message,
                ParseableSourceType.Tag => (_commitShaToTagName.TryGetValue(commit.Sha, out var tag) ? tag : string.Empty),
                _ => throw new NotImplementedException($"Parseable Source Type {sourceType} is not implemented")
            };
        }

        protected string GetMatchOrDefault(ParseableProperty property, Commit commit, string defaultValue = "")
        {
            defaultValue = property.IsOptional ? defaultValue : null;
            string source = GetSource(commit, property.SourceType);
            var match = Regex.Match(source, property.Pattern);
            if (!match.Success) return defaultValue;
            var result = match.Groups[1].Value.Trim();
            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }
        
        protected string HandleVersion(string version)
        {
            if (string.Equals(version, Unreleased, StringComparison.CurrentCultureIgnoreCase))
            {
                version = LastVersion;
            }
            else
            {
                LastVersion = version;
            }

            return version;
        }
    }
}