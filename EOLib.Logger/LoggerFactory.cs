using System;
using System.Linq;
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

        public ILogger CreateLogger<TLogger>(string fileName = "")
            where TLogger : class, ILogger, new()
        {
            var constructors = typeof(TLogger).GetConstructors()
                .Where(x => x.GetParameters().Length == 1 && !x.IsPrivate)
                .Select(x => x.GetParameters()[0].ParameterType);

            if (constructors.Any(x => x == typeof(IConfigurationProvider)))
                return (TLogger)Activator.CreateInstance(typeof(TLogger), _configurationProvider);
            else if (constructors.Any(x => x == typeof(string)))
                return (TLogger)Activator.CreateInstance(typeof(TLogger), fileName);

            return new TLogger();
        }
    }

    public interface ILoggerFactory
    {
        ILogger CreateLogger<TLogger>(string fileName = "")
            where TLogger : class, ILogger, new();
    }
}