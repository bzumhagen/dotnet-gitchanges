using System;

namespace Gitchanges.Changes
{
    public class OverrideChange : BaseChange
    {
        public string Id { get; }
        public OverrideChange(string id, string version, string changeType, string summary, DateTimeOffset date, string reference = "") : base(version, changeType, summary, date, reference)
        {
            Id = EnsureNonEmpty(id, nameof(id));
        }
    }
}