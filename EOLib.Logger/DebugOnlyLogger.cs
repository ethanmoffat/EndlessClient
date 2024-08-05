using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EOLib.Config;

namespace EOLib.Logger
{
    public class DebugOnlyLogger : ILogger
    {
        private readonly object _fileStreamLock = new object();

        private readonly Timer _logTimer;
        private readonly bool _enabled;

        private StreamWriter _fileStream;
        private int _writesSinceLastFlush;

        public DebugOnlyLogger() { }

        internal DebugOnlyLogger(IConfigurationProvider configurationProvider)
        {
            if (!configurationProvider.EnableLog)
                return;

            _enabled = true;

            Directory.CreateDirectory(Constants.LOG_DIRECTORY);
            _fileStream = new StreamWriter(Constants.LOG_FILE_PATH, true);

            _logTimer = new Timer(FlushLogToNewFileIfNeeded,
                null,
                Constants.FLUSH_TIME_MS,
                Constants.FLUSH_TIME_MS);

            WriteFileOpenedHeader();
        }

        public void Log(string format, params object[] parameters)
        {
            if (!_enabled) return;

            var threadID = Thread.CurrentThread.ManagedThreadId;
            var processID = Process.GetCurrentProcess().Id;
            var front = $"{processID,-5} {threadID,5} {GetDateTimeString(),-25} {format}";

            WriteToFile(string.Format(front, parameters));
        }

        private void FlushLogToNewFileIfNeeded(object state)
        {
            if (!_enabled || _writesSinceLastFlush <= 0)
                return;

            lock (_fileStreamLock)
            {
                _writesSinceLastFlush = 0;
                _fileStream.Close();

                //check if the file should be split (based on length)
                var append = true;
                var fi = new FileInfo(Constants.LOG_FILE_PATH);
                if (fi.Length > Constants.SPLIT_FILE_BYTE_LENGTH)
                {
                    append = false;
                    var fileName = string.Format(Constants.LOG_FILE_FMT, DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss"));
                    File.Copy(Constants.LOG_FILE_PATH, fileName);
                }

                _fileStream = new StreamWriter(Constants.LOG_FILE_PATH, append);
            }
        }

        private void WriteFileOpenedHeader()
        {
            _fileStream.WriteLine("------------------------------------------------------------------");
            _fileStream.WriteLine("Log opened at {0}", GetDateTimeString());
            _fileStream.WriteLine("Process ID - Managed Thread ID - DateTime - Info");
            _fileStream.WriteLine("------------------------------------------------------------------");
        }

        private string GetDateTimeString()
        {
            return DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss:fff");
        }

        private void WriteToFile(string logStr)
        {
            lock (_fileStreamLock)
            {
                _fileStream.WriteLine(logStr);
                _writesSinceLastFlush++;

                if (_writesSinceLastFlush > Constants.FLUSH_LINES_WRITTEN)
                {
                    _fileStream.Close();
                    _fileStream = new StreamWriter(Constants.LOG_FILE_PATH, true);
                    _writesSinceLastFlush = 0;
                }
            }
        }

        ~DebugOnlyLogger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _enabled)
            {
                _logTimer.Dispose();
                _fileStream.Dispose();
            }
        }
    }
}