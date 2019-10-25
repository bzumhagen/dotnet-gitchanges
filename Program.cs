using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Stubble.Core.Builders;

namespace dotnet_gitchanges
{
    class Program
    {
        static void Main(string[] args)
        {
            var stubble = new StubbleBuilder().Build();
            List<Dictionary<string, string>> changes = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
                {
                    {"summary", "This is a test"}
                }
            };
            List<Dictionary<string, object>> tags = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"tag", "Added"},
                    {"changes", changes}
                }
            };
            List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"version", "0.1.0"},
                    {"date", "2019-10-24"},
                    {"tags", tags}
                }
            };
            Dictionary<string, object> versionsDict = new Dictionary<string, object>()
            {
                {"versions", versions}
            };
            
            var template = Encoding.UTF8.GetString(Resource.KeepAChangelogTemplate);
            var output = stubble.Render(template, versionsDict);
            File.WriteAllText(@"changelog.md", output);
            /*using (var repo = new Repository("."))
            {
                var RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";

                foreach (Commit c in repo.Commits.Take(15))
                {
                    Console.WriteLine($"commit {c.Id}");
                   
                    if (c.Parents.Count() > 1)
                    {
                        Console.WriteLine("Merge: {0}", 
                            string.Join(" ", c.Parents.Select(p => p.Id.Sha.Substring(0, 7)).ToArray()));
                    }

                    Console.WriteLine($"Author: {c.Author.Name} <{c.Author.Email}>");
                    Console.WriteLine("Date:   {0}", c.Author.When.ToString(RFC2822Format, CultureInfo.InvariantCulture));
                    Console.WriteLine();
                    Console.WriteLine(c.Message);
                    Console.WriteLine();
                }
            }*/
        }
    }
}
