using System;

namespace Gitchanges.Changes
{
    public class ProjectChange : BaseChange
    {
        public string Project { get; }
        
        public ProjectChange(string project, string version, string changeType, string summary, DateTimeOffset date, string reference = "") : base(version, changeType, summary, date, reference)
        {
            Project = EnsureNonEmpty(project, nameof(project));
        }
    }
}