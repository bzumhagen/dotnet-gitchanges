using System.Collections.Generic;
using System.IO;
using Gitchanges.Readers.Parsers;

namespace Gitchanges.Readers
{
    public class FileReader<T> : IGenericReader<T>
    {
        private readonly string _path;
        private readonly IRowParser<T> _rowParser;

        public FileReader(string pathToFile, IRowParser<T> rowParser)
        {
            _path = pathToFile;
            _rowParser = rowParser;
        }
        
        public IEnumerable<T> Values()
        {
            using (var file = new StreamReader(_path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    var value = _rowParser.Parse(line);
                    if (value == null) continue;

                    yield return value;
                } 
            }
        }
        
    }
}