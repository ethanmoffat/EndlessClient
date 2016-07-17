// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;
using Microsoft.Practices.Unity;

namespace EOLib.Net
{
    public class NetworkDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IPacketEncoderService, PacketEncoderService>();
            container.RegisterType<IPacketSequenceService, PacketSequenceService>();
            container.RegisterType<IHashService, HashService>();
            container.RegisterType<IPacketSendService, PacketSendService>();
            container.RegisterType<IPacketHandlingTypeFinder, PacketHandlingTypeFinder>();

            //the repository is a "disposer" of the NetworkClient (so NetworkClient gets cleaned up later if it is set)
            container.RegisterInstance<INetworkClientDisposer, NetworkClientRepository>();

            container.RegisterType<INetworkClientFactory, NetworkClientFactory>();
            container.RegisterType<ISafeInBandNetworkOperationFactory, SafeInBandNetworkOperationFactory>();

            container.RegisterInstance<INetworkClientRepository, NetworkClientRepository>();
            container.RegisterInstance<INetworkClientProvider, NetworkClientRepository>();
            container.RegisterInstance<IPacketQueueRepository, PacketQueueRepository>();
            container.RegisterInstance<IPacketQueueProvider, PacketQueueRepository>();
            container.RegisterInstance<IPacketEncoderRepository, PacketEncoderRepository>();
            container.RegisterInstance<ISequenceRepository, SequenceRepository>();
            container.RegisterInstance<IConnectionStateRepository, ConnectionStateRepository>();
            container.RegisterInstance<IConnectionStateProvider, ConnectionStateRepository>();
            container.RegisterInstance<IPacketHandlerRepository, PacketHandlerRepository>();
            container.RegisterInstance<IPacketHandlerProvider, PacketHandlerRepository>();

            container.RegisterType<IPacketProcessorActions, PacketProcessActions>();
            container.RegisterType<INetworkConnectionActions, NetworkConnectionActions>();
            container.RegisterType<IPacketHandlingActions, PacketHandlingActions>();
            
            //must be a singleton: tracks a thread and has internal state.
            container.RegisterInstance<IBackgroundReceiveActions, BackgroundReceiveActions>();

            //packet handling
            container.RegisterInstance<IOutOfBandPacketHandler, OutOfBandPacketHandler>();
            container.RegisterType<IPacketHandlerFinderService, PacketHandlerFinderService>();

            container.RegisterVaried<IPacketHandler, ConnectionPlayerHandler>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //create the client object (can be recreated later)
            var clientRepo = container.Resolve<INetworkClientRepository>();
            var clientFactory = container.Resolve<INetworkClientFactory>();
            clientRepo.NetworkClient = clientFactory.CreateNetworkClient();
        }
    }
}
