// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Logger
{
    public class LoggerProvider : ILoggerProvider
    {
        public ILogger Logger { get; private set; }

        internal LoggerProvider(ILoggerFactory loggerFactory)
        {
            Logger = CreateLogger(loggerFactory);
        }

        private static ILogger CreateLogger(ILoggerFactory loggerFactory)
        {
#if DEBUG
            return loggerFactory.CreateLogger();
#else
            return new NullLogger();
#endif
        }

        public void Dispose()
        {
            Logger.Dispose();
        }
    }

    public interface ILoggerProvider : IDisposable
    {
        ILogger Logger { get; }
    }
}
