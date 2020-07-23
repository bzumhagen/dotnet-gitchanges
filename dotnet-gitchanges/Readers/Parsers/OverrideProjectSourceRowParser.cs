using System;
using System.Globalization;
using System.IO;
using Gitchanges.Changes;

namespace Gitchanges.Readers.Parsers
{
    public class OverrideProjectSourceRowParser : IRowParser<OverrideProjectChange>
    {
        private readonly TextWriter _errorWriter;
        private const char Delimiter = '|';
        private const string DateFormat = "yyyy-MM-dd";
        
        public OverrideProjectSourceRowParser(TextWriter errorWriter)
        {
            _errorWriter = errorWriter;
        }
        
        public OverrideProjectChange Parse(string line)
        {
            var values = line.Split(Delimiter);
            OverrideProjectChange change = null;
            
            try
            {
                switch(values.Length)
                {
                    case 6:
                        change = new OverrideProjectChange(id: values[0], project: values[1], version: new ChangeVersion(values[2]), changeType: values[3], summary: values[4], date: DateTimeOffset.ParseExact(values[5], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    case 7:
                        change = new OverrideProjectChange(id: values[0], project: values[1], reference: values[2], version: new ChangeVersion(values[3]), changeType: values[4], summary: values[5], date: DateTimeOffset.ParseExact(values[6], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    default:
                        throw new ArgumentException($"Wrong number of values. Expected 6 or 7 but was {values.Length}");
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