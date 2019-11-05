using System;
using System.IO;
using System.Reflection;
using System.Text;
using Gitchanges.Caches;
using Gitchanges.Configuration;
using Gitchanges.Readers;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Stubble.Core.Builders;
using CommandLine;

namespace Gitchanges
{
    class Program
    {
        public class Options
        {
            [Option('s', "settings", Required = false, HelpText = "Path to custom settings file.")]
            public string CustomSettingsPath { get; set; }
        }
        
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    if (!string.IsNullOrEmpty(options.CustomSettingsPath))
                    {
                        configBuilder.AddJsonFile(options.CustomSettingsPath);
                    }
                });
            
            var config = TryOrExit(() => configBuilder.Build(), "Failed to build configuration");
            var patterns = config.GetSection("Parsing").Get<ParsingPatterns>();
            var stubble = new StubbleBuilder().Build();
            var template = TryOrExit(() =>
            {
                var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Gitchanges.KeepAChangelogTemplate.Mustache");
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
