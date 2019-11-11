using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Gitchanges.Changes;

namespace Gitchanges.Readers
{
    public class FileReader : IRepositoryReader
    {
        private readonly string _path;
        private readonly char _delimiter;
        private readonly TextWriter _errorWriter;
        private const string DateFormat = "yyyy-MM-dd";

        public FileReader(string pathToFile, char valueDelimiter, TextWriter errorWriter)
        {
            _path = pathToFile;
            _delimiter = valueDelimiter;
            _errorWriter = errorWriter;
        }
        
        public IEnumerable<IChange> Changes()
        {
            using (var file = new StreamReader(_path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    var change = ParseLine(line);
                    if (change == null) continue;

                    yield return change;
                } 
            }
        }

        private IChange ParseLine(string line)
        {
            var values = line.Split(_delimiter);
            IChange change = null;
            
            try
            {
                switch(values.Length)
                {
                    case 4:
                        change = new GitChange(version: values[0], tag: values[1], summary: values[2], date: DateTimeOffset.ParseExact(values[3], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    case 5:
                        change = new GitChange(reference: values[0], version: values[1], tag: values[2], summary: values[3], date: DateTimeOffset.ParseExact(values[4], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    default:
                        throw new ArgumentException($"Wrong number of values. Expected 4 or 5 but was {values.Length}");
                }
            }
            catch (Exception e)
            {
                var message = e.Message;
                if (e.GetType() == typeof(FormatException))
                {
                    message = $"Date should match the format '{DateFormat}'";
                }
                
                _errorWriter.WriteLine($"Error parsing line '{line}'. {message}");
            }
            
            return change;
        }
        
    }
}