// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Config;

namespace EOLib.Logger
{
    internal class LoggerFactory : ILoggerFactory
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

    internal interface ILoggerFactory
    {
        ILogger CreateLogger();
    }
}
