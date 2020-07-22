using Gitchanges.Changes;
using Gitchanges.Configuration;
using LibGit2Sharp;

namespace Gitchanges.Readers.Parsers
{
    public class ProjectCommitParser : DefaultCommitParser
    {
        private const string Global = "Global";
        public ProjectCommitParser(ParsingConfig parsingConfig) : base(parsingConfig)
        {
        }

        public new ProjectChange Parse(Commit commit)
        {
            var reference = GetMatchOrDefault(ParsingConfig.Reference, commit);
            var changeType = GetMatchOrDefault(ParsingConfig.ChangeType, commit, Uncategorized);
            var version = GetMatchOrDefault(ParsingConfig.Version, commit, LastVersion);
            var project = GetMatchOrDefault(ParsingConfig.Project, commit, Global);

            if (reference == null || changeType == null || version == null || project == null)
            {
                return null;
            }

            version = HandleVersion(version);
            
            return new ProjectChange(project, version, changeType, commit.MessageShort, commit.Author.When, reference);
        }
    }
}