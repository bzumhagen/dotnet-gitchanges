using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using Gitchanges.Caches;
using Gitchanges.Configuration;
using Gitchanges.Readers;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Stubble.Core.Builders;

namespace Gitchanges
{
    class Program
    {
        public class Options
        {
            [Option('s', "settings", Required = false, HelpText = "Path to custom settings file.")]
            public string CustomSettingsPath { get; set; }
            [Option('t', "template", Required = false, HelpText = "Path to custom template file. Overrides value specified in custom settings file.")]
            public string CustomTemplatePath { get; set; }
            [Option('e', "exclude", Required = false, HelpText = "Comma separated tags to exclude. Overrides value specified in custom settings file.")]
            public string TagsToExclude { get; set; }
        }
        
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    var additionalSettings = new List<KeyValuePair<string, string>>();

                    if (!string.IsNullOrEmpty(options.CustomSettingsPath)) 
                        configBuilder.AddJsonFile(options.CustomSettingsPath);

                    if (!string.IsNullOrEmpty(options.CustomTemplatePath))
                        additionalSettings.Add(new KeyValuePair<string, string>("Template", options.CustomTemplatePath));
                    
                    if (!string.IsNullOrEmpty(options.TagsToExclude))
                        additionalSettings.Add(new KeyValuePair<string, string>("TagsToExclude", options.TagsToExclude));

                    configBuilder.AddInMemoryCollection(additionalSettings);
                });
            
            var config = TryOrExit(() => configBuilder.Build(), "Failed to build configuration");
            var patterns = config.GetSection("Parsing").Get<ParsingPatterns>();
            var templatePath = config.GetSection("Template").Value;
            var template = GetTemplate(templatePath);
            var repo = TryOrExit(() => new Repository("."), "Failed to initialize repository");
            var cache = new ChangeCache();
            var reader = new GitReader(repo, patterns);
            var tagsToExclude = (config.GetSection("TagsToExclude").Value ?? "").Split(",").Select(tag => tag.ToLower()).ToHashSet();
            
            foreach (var change in reader.Changes())
            {
                if (!tagsToExclude.Contains(change.Tag.ToLower())) cache.Add(change);
            }

            var results = cache.GetAsValueDictionary();
            var stubble = new StubbleBuilder().Build();
            var output = stubble.Render(template, results);
            File.WriteAllText(@"changelog.md", output);
        }

        private static string GetTemplate(string templatePath)
        {
            return TryOrExit(() =>
            {
                var stream = string.IsNullOrEmpty(templatePath) ? Assembly.GetEntryAssembly()?.GetManifestResourceStream("Gitchanges.KeepAChangelogTemplate.Mustache") : File.OpenRead(templatePath);
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }, "Failed to read template file");
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
