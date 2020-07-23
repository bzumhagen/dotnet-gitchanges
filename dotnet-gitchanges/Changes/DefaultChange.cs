using System;

namespace Gitchanges.Changes
{
    public class DefaultChange : BaseChange
    {
        public DefaultChange(ChangeVersion version, string changeType, string summary, DateTimeOffset date, string reference = "") : base(version, changeType, summary, date, reference){}

        public override string ToString()
        {
            return $"DefaultChange {{ Version: {Version}, ChangeType: {ChangeType}, Summary: {Summary}, Date: {Date:yyyy-MM-dd}, Reference: {Reference} }}";
        }
    }
}