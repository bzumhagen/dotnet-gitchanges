using System;
using System.Collections.Generic;

namespace Gitchanges.Configuration
{
    public class RepositoryConfig
    {
        public class ChangeOverride {
            public string Id { get; set; }
            public string Version { get; set; }
            public string Tag { get; set; }
            public string Summary { get; set; }
            public DateTimeOffset Date { get; set; }
            public string Reference { get; set; }
        }
        public string Path { get; set; }
        public IEnumerable<ChangeOverride> ChangeOverrides { get; set; }
    }
}