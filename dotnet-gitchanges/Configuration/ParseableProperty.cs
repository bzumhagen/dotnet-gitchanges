namespace Gitchanges.Configuration
{
	public class ParseableProperty
	{
		public ParseableSourceType SourceType { get; set; }
		public string Pattern { get; set; }
		public bool IsOptional { get; set; }
	}
}