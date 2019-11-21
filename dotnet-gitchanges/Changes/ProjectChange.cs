using System;

namespace Gitchanges.Changes
{
    public class ProjectChange : BaseChange
    {
        public string Project { get; }
        
        public ProjectChange(string project, string version, string tag, string summary, DateTimeOffset date, string reference = "") : base(version, tag, summary, date, reference)
        {
            Project = EnsureNonEmpty(project, nameof(project));
        }
    }
}