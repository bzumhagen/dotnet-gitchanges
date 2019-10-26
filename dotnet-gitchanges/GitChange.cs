using System;

namespace dotnet_gitchanges
{
    public class GitChange : IChange
    {
        public string Version { get; }
        public string Tag { get; }
        public string Summary { get; }
        public DateTime Date { get; }
        public string Reference { get; }

        public GitChange(string version, string tag, string summary, DateTime date, string reference = "")
        {
            Version = version;
            Tag = tag;
            Summary = summary;
            Date = date;
            Reference = reference;
        }

        protected bool Equals(GitChange other)
        {
            return Version == other.Version && Tag == other.Tag && Summary == other.Summary && Date == other.Date && Reference == other.Reference;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GitChange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tag != null ? Tag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Summary != null ? Summary.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Date != null ? Date.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Reference != null ? Reference.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}