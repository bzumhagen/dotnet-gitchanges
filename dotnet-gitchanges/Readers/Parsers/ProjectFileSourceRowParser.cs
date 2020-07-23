using System;
using System.Globalization;
using System.IO;
using Gitchanges.Changes;

namespace Gitchanges.Readers.Parsers
{
    public class ProjectFileSourceRowParser : IRowParser<ProjectChange>
    {
        private readonly TextWriter _errorWriter;
        private const char Delimiter = '|';
        private const string DateFormat = "yyyy-MM-dd";
        
        public ProjectFileSourceRowParser(TextWriter errorWriter)
        {
            _errorWriter = errorWriter;
        }
        
        public ProjectChange Parse(string line)
        {
            var values = line.Split(Delimiter);
            ProjectChange change = null;
            
            try
            {
                switch(values.Length)
                {
                    case 5:
                        change = new ProjectChange(project: values[0], version: new ChangeVersion(values[1]), changeType: values[2], summary: values[3], date: DateTimeOffset.ParseExact(values[4], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    case 6:
                        change = new ProjectChange(project: values[0], reference: values[1], version: new ChangeVersion(values[2]), changeType: values[3], summary: values[4], date: DateTimeOffset.ParseExact(values[5], DateFormat, CultureInfo.InvariantCulture));
                        break;
                    default:
                        throw new ArgumentException($"Wrong number of values. Expected 5 or 6 but was {values.Length}");
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