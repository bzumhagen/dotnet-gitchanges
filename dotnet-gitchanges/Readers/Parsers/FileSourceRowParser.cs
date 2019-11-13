using System;
using System.Globalization;
using System.IO;
using Gitchanges.Changes;

namespace Gitchanges.Readers.Parsers
{
    public class FileSourceRowParser : IRowParser<IChange>
    {
        private readonly TextWriter _errorWriter;
        private const char Delimiter = '|';
        private const string DateFormat = "yyyy-MM-dd";
        
        public FileSourceRowParser(TextWriter errorWriter)
        {
            _errorWriter = errorWriter;
        }
        
        public IChange Parse(string line)
        {
            var values = line.Split(Delimiter);
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