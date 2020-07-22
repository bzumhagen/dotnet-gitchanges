namespace Gitchanges.Configuration
{
    public class AppConfig
    {
        public ParsingConfig Parsing { get; set; } 
        public string Template { get; set; }
        public string ChangeTypesToExclude { get; set; }
        public string MinVersion { get; set; }
        public RepositoryConfig Repository { get; set; }
        public string FileSource { get; set; }
        public bool MultiProject { get; set; }
    }
}