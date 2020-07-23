using System;

namespace Gitchanges.Changes
{
    public abstract class BaseChange : IChange
    {
        public ChangeVersion Version { get; }
        public string ChangeType { get; }
        public string Summary { get; }
        public DateTimeOffset Date { get; }
        public string Reference { get; }

        protected BaseChange(ChangeVersion version, string changeType, string summary, DateTimeOffset date, string reference = "")
        {
            Version = version;
            ChangeType = EnsureNonEmpty(changeType, nameof(changeType));
            Summary = EnsureNonEmpty(summary, nameof(summary));
            Date = EnsureNonNull(date, nameof(date));
            Reference = reference;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ChangeType != null ? ChangeType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Summary != null ? Summary.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Date.GetHashCode();
                hashCode = (hashCode * 397) ^ (Reference != null ? Reference.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseChange) obj);
        }
        
        protected bool Equals(BaseChange other)
        {
            return Version.Equals(other.Version) && ChangeType == other.ChangeType && Summary == other.Summary && Date.Equals(other.Date) && Reference == other.Reference;
        }

        protected static T EnsureNonNull<T>(T obj, string name)
        {
            if (obj == null) throw new ArgumentException($"Parameter {name} cannot be null");
            return obj;
        }
        
        protected static string EnsureNonEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"String parameter {name} cannot be null or empty.");
            return value;
        }
    }
}