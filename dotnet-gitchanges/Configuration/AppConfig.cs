namespace Gitchanges.Configuration
{
    public class AppConfig
    {
        public ParsingPatterns Parsing { get; set; } 
        public string Template { get; set; }
        public string TagsToExclude { get; set; }
        public string MinVersion { get; set; }
        public RepositoryConfig Repository { get; set; }
        public string FileSource { get; set; }
        public bool MultiProject { get; set; }
        public bool VersionFromGitTag { get; set; }
    }
}