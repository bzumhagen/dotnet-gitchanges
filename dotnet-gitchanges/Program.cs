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
            var template = Encoding.UTF8.GetString(Resource.KeepAChangelogTemplate);
            var v = new Versions();
            v.Add(new GitChange("0.1.0", "Removed", "Did a thing", DateTime.Today.AddDays(-2)));
            v.Add(new GitChange("0.1.0", "Added", "Did another thing", DateTime.Today.AddDays(-1)));
            v.Add(new GitChange("0.2.0", "Added", "Did another done thing earlier", DateTime.Today.AddDays(-1)));
            v.Add(new GitChange("0.2.0", "Added", "Did another done thing", DateTime.Today));
            var results = v.GetAsValueDictionary();
            var output = stubble.Render(template, results);
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
