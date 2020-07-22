using System;

namespace Gitchanges.Changes
{
    public class OverrideProjectChange : ProjectChange
    {
        public string Id { get; }
        public OverrideProjectChange(string id, string project, string version, string changeType, string summary, DateTimeOffset date, string reference = "") : base(project, version, changeType, summary, date, reference)
        {
            Id = EnsureNonEmpty(id, nameof(id));
        }
    }
}