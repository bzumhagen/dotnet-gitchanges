using System;

namespace Gitchanges.Changes
{
    public class DefaultChange : BaseChange
    {
        public DefaultChange(string version, string tag, string summary, DateTimeOffset date, string reference = "") : base(version, tag, summary, date, reference){}

        public override string ToString()
        {
            return $"DefaultChange {{ Version: {Version}, Tag: {Tag}, Summary: {Summary}, Date: {Date:yyyy-MM-dd}, Reference: {Reference} }}";
        }
    }
}