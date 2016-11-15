// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Logger
{
    public class Logger : ILogger
    {
        internal Logger()
        {
            
        }

        public void Log(string format, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        ~Logger()
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
        }
    }

    public interface ILogger : IDisposable
    {
        void Log(string format, params object[] parameters);
    }
}
