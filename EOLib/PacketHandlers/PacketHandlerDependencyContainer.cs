// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Net.Handlers;
using Microsoft.Practices.Unity;

namespace EOLib.PacketHandlers
{
    public class PacketHandlerDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterVaried<IPacketHandler, ConnectionPlayerHandler>()
                .RegisterVaried<IPacketHandler, PingResponseHandler>()
                .RegisterVaried<IPacketHandler, FindCommandPlayerNotFoundHandler>()
                .RegisterVaried<IPacketHandler, FindCommandPlayerSameMapHandler>()
                .RegisterVaried<IPacketHandler, FindCommandPlayerDifferentMapHandler>();

            container.RegisterVaried<IPacketHandler, PublicChatHandler>()
                .RegisterVaried<IPacketHandler, GroupChatHandler>();
        }
    }
}
