using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using LibGit2Sharp;
using Stubble.Core.Builders;

namespace dotnet_gitchanges
{
    class Program
    {
        static void Main(string[] args)
        {
            var stubble = new StubbleBuilder().Build();
            var template = Encoding.UTF8.GetString(Resource.KeepAChangelogTemplate);
            var repo = new Repository(".");
            var cache = new ChangeCache();
            var reader = new GitReader(repo, cache);
            reader.LoadCache();

            var results = cache.GetAsValueDictionary();
            var output = stubble.Render(template, results);
            File.WriteAllText(@"changelog.md", output);
        }
    }
}
