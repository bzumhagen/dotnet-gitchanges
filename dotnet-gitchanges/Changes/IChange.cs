using System;

namespace Gitchanges.Changes
{
    public interface IChange
    {
        ChangeVersion Version { get; }
        string ChangeType { get; }
        string Summary { get; }
        DateTimeOffset Date { get; }
        string Reference { get; }
    }
}