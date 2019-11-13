using System;

namespace Gitchanges.Changes
{
    public class OverrideChange : BaseChange
    {
        public string Id { get; }
        public OverrideChange(string id, string version, string tag, string summary, DateTimeOffset date, string reference = "") : base(version, tag, summary, date, reference)
        {
            Id = EnsureNonEmpty(id, nameof(id));
        }
    }
}