namespace Gitchanges.Configuration
{
    public class ParsingConfig
    {
        public ParseableProperty Reference { get; set; }
        public ParseableProperty Version { get; set; }
        public ParseableProperty ChangeType { get; set; }
        public ParseableProperty Project { get; set; }
    }
}