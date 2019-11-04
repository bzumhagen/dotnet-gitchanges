using System;
using System.IO;
using System.Reflection;
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
            
            var template = TryOrExit(() =>
            {
                var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("dotnet_gitchanges.KeepAChangelogTemplate.Mustache");
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }, "Failed to read template file");
            var repo = TryOrExit(() => new Repository("."), "Failed to initialize repository");
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

        private static T TryOrExit<T>(Func<T> action, string failureMessage)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{failureMessage}: {e.Message}");
                Environment.Exit(-1);
            }

            return default;
        }
    }
}
