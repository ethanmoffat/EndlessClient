// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Logger
{
    public class LoggerProvider : ILoggerProvider
    {
        public ILogger Logger { get; private set; }

        internal LoggerProvider(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger();
        }
    }

    public interface ILoggerProvider
    {
        ILogger Logger { get; }
    }
}
