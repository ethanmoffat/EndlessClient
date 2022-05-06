using System;
using System.IO;

namespace EOLib.Logger
{
    public class FileLogger : ILogger
    {
        private readonly bool _enabled;
        private StreamWriter _fileStream;

        public FileLogger() { }

        internal FileLogger(string fileName)
        {
            _enabled = true;
            _fileStream = new StreamWriter(fileName, append: true);
        }

        public void Log(string format, params object[] parameters)
        {
            var dateFormat = DateTime.Now.ToString("G");
            _fileStream.WriteLine($"{dateFormat} {format}", parameters);
        }

        public void Dispose()
        {
            if (_enabled)
            {
                _fileStream.Dispose();
            }
        }
    }
}
