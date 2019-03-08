// Original Work Copyright (c) Ethan Moffat 2014-2019
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EOLib.Config;

namespace EndlessClient.Initialization
{
    [MappedType(BaseType = typeof(IGameInitializer))]
    public class ConfigInitializer : IGameInitializer
    {
        private readonly IConfigFileLoadActions _configFileLoadActions;

        public ConfigInitializer(IConfigFileLoadActions configFileLoadActions)
        {
            _configFileLoadActions = configFileLoadActions;
        }

        public void Initialize()
        {
            _configFileLoadActions.LoadConfigFile();
        }
    }

    //required for the varied type registration to work until more initializers are added
    [MappedType(BaseType = typeof(IGameInitializer))]
    public class NoopInitializer : IGameInitializer
    {
        public void Initialize()
        {
        }
    }
}
