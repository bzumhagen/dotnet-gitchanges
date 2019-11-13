using System;

namespace Gitchanges.Changes
{
    public class GitChange : BaseChange
    {
        public GitChange(string version, string tag, string summary, DateTimeOffset date, string reference = "") : base(version, tag, summary, date, reference){}

        public override string ToString()
        {
            return $"GitChange {{ Version: {Version}, Tag: {Tag}, Summary: {Summary}, Date: {Date:yyyy-MM-dd}, Reference: {Reference} }}";
        }
    }
}