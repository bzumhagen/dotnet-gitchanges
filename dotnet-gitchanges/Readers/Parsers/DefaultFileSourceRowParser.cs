using System;
using System.Globalization;
using System.IO;
using Gitchanges.Changes;

namespace Gitchanges.Readers.Parsers
{
    public class DefaultFileSourceRowParser : IRowParser<DefaultChange>
    {
        private readonly TextWriter _errorWriter;
        private const char Delimiter = '|';
        private const string DateFormat = "yyyy-MM-dd";
        
        public DefaultFileSourceRowParser(TextWriter errorWriter)
        {
            _errorWriter = errorWriter;
        }
        
        public DefaultChange Parse(string line)
        {
            var values = line.Split(Delimiter);
            DefaultChange change = null;
            
            try
            {
                switch(values.Length)
                {
                    case 4:
                        change = new DefaultChange(version: new ChangeVersion(values[0]), changeType: values[1], summary: values[2], date: DateTimeOffset.ParseExact(values[3], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    case 5:
                        change = new DefaultChange(reference: values[0], version: new ChangeVersion(values[1]), changeType: values[2], summary: values[3], date: DateTimeOffset.ParseExact(values[4], DateFormat, CultureInfo.InvariantCulture));
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