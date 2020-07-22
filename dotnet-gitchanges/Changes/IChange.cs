using System;

namespace Gitchanges.Changes
{
    public interface IChange
    {
        string Version { get; }
        string ChangeType { get; }
        string Summary { get; }
        DateTimeOffset Date { get; }
        string Reference { get; }
    }
}