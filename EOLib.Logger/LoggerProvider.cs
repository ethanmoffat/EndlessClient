using AutomaticTypeMapper;
using System;

namespace EOLib.Logger
{
    [MappedType(BaseType = typeof(ILoggerProvider), IsSingleton = true)]
    public class LoggerProvider : ILoggerProvider
    {
        public ILogger Logger { get; }

        public LoggerProvider(ILoggerFactory loggerFactory)
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
