// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Config;
using EOLib.DependencyInjection;
using EOLib.Domain;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Translators;

namespace EndlessClient
{
    public static class DependencyContainerProvider
    {
        public static readonly IDependencyContainer[] DependencyContainers =
        {
            //EOLib containers
            new ConfigDependencyContainer(),
            new DomainDependencyContainer(),
            new EOLibDependencyContainer(),
            new GraphicsDependencyContainer(),
            new IODependencyContainer(),
            new LocalizationDependencyContainer(),
            new NetworkDependencyContainer(),
            new PacketTranslatorContainer(),

            //EndlessClient containers
            new EndlessClientDependencyContainer(),
            new XNAControlsDependencyContainer()
        };
    }
}
