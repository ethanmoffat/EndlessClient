// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Net.Builders;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.FileTransfer;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;
using Unity;

namespace EOLib.Net
{
    public class NetworkDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IPacketEncoderService, PacketEncoderService>()
                .RegisterType<IPacketSequenceService, PacketSequenceService>()
                .RegisterType<IHashService, HashService>()
                .RegisterType<IPacketSendService, PacketSendService>()
                .RegisterType<IFileRequestService, FileRequestService>();

            //the repository is a "disposer" of the NetworkClient (so NetworkClient gets cleaned up later if it is set)
            container.RegisterInstance<INetworkClientDisposer, NetworkClientRepository>();

            container.RegisterType<INetworkClientFactory, NetworkClientFactory>()
                .RegisterType<ISafeNetworkOperationFactory, SafeNetworkOperationFactory>();

            container.RegisterInstance<INetworkClientRepository, NetworkClientRepository>()
                .RegisterInstance<INetworkClientProvider, NetworkClientRepository>()
                .RegisterInstance<IPacketQueueRepository, PacketQueueRepository>()
                .RegisterInstance<IPacketQueueProvider, PacketQueueRepository>()
                .RegisterInstance<IPacketEncoderRepository, PacketEncoderRepository>()
                .RegisterInstance<ISequenceRepository, SequenceRepository>()
                .RegisterInstance<IConnectionStateRepository, ConnectionStateRepository>()
                .RegisterInstance<IConnectionStateProvider, ConnectionStateRepository>()
                .RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>()
                .RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>();

            container.RegisterType<IPacketProcessActions, PacketProcessActions>()
                .RegisterType<INetworkConnectionActions, NetworkConnectionActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>();
            
            container.RegisterType<IBackgroundReceiveActions, BackgroundReceiveActions>()
                .RegisterInstance<IBackgroundReceiveThreadRepository, BackgroundReceiveThreadRepository>();

            container.RegisterType<IChatPacketBuilder, ChatPacketBuilder>();

            RegisterPacketHandlerDependencies(container);
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //create the client object (can be recreated later)
            //constructor-injecting the factory causes a dependency loop.
            var clientRepo = container.Resolve<INetworkClientRepository>();
            var clientFactory = container.Resolve<INetworkClientFactory>();
            clientRepo.NetworkClient = clientFactory.CreateNetworkClient();
        }

        private static void RegisterPacketHandlerDependencies(IUnityContainer container)
        {
            container.RegisterInstance<IOutOfBandPacketHandler, OutOfBandPacketHandler>()
                .RegisterType<IPacketHandlerFinder, PacketHandlerFinder>()
                .RegisterType<IPacketHandlingActions, PacketHandlingActions>()
                .RegisterInstance<IPacketHandlerProvider, PacketHandlerProvider>()
                .RegisterType<IPacketHandlingTypeFinder, PacketHandlingTypeFinder>();
        }
    }
}
