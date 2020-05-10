using AutomaticTypeMapper;
using EOLib.Config;

namespace EOLib.Logger
{
    [MappedType(BaseType = typeof(ILoggerFactory))]
    public class LoggerFactory : ILoggerFactory
    {
        private readonly IConfigurationProvider _configurationProvider;

        public LoggerFactory(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public ILogger CreateLogger()
        {
            return new DebugOnlyLogger(_configurationProvider);
        }
    }

    public interface ILoggerFactory
    {
        ILogger CreateLogger();
    }
}
