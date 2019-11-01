using System.IO;
using System.Text;
using dotnet_gitchanges.Configuration;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Stubble.Core.Builders;

namespace dotnet_gitchanges
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var patterns = config.GetSection("Parsing").Get<ParsingPatterns>();
            
            var stubble = new StubbleBuilder().Build();
            var template = Encoding.UTF8.GetString(Resource.KeepAChangelogTemplate);
            var repo = new Repository(".");
            var cache = new ChangeCache();
            var reader = new GitReader(repo, patterns);

            foreach (var change in reader.Changes())
            {
                cache.Add(change);
            }

            var results = cache.GetAsValueDictionary();
            var output = stubble.Render(template, results);
            File.WriteAllText(@"changelog.md", output);
        }
    }
}
