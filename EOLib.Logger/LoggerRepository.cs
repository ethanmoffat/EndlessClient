// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Logger
{
    public class LoggerRepository : ILoggerRepository, ILoggerProvider
    {
        public ILogger Logger { get; set; }
    }

    public interface ILoggerProvider
    {
        ILogger Logger { get; }
    }

    public interface ILoggerRepository
    {
        ILogger Logger { get; set; }
    }
}
