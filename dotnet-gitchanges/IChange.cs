using System;

namespace dotnet_gitchanges
{
    public interface IChange
    {
        string Version { get; }
        string Tag { get; }
        string Summary { get; }
        DateTime Date { get; }
        string Reference { get; }
    }
}