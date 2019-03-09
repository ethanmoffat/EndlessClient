﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.DependencyInjection;
using EOLib.Domain;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Translators;
using EOLib.PacketHandlers;

namespace EndlessClient
{
    public static class DependencyContainerProvider
    {
        public static readonly IDependencyContainer[] DependencyContainers =
        {
            //EOLib containers
            new DomainDependencyContainer(),
            new EOLibDependencyContainer(),
            new NetworkDependencyContainer(),
            new NotifierDependencyContainer(),
            new PacketTranslatorContainer(),
            new PacketHandlerDependencyContainer()
        };
    }
}
