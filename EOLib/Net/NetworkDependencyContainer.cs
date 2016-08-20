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
using Microsoft.Practices.Unity;

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
                .RegisterType<IPacketHandlingTypeFinder, PacketHandlingTypeFinder>()
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
                .RegisterInstance<IPacketHandlerProvider, PacketHandlerProvider>()
                .RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>()
                .RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>();

            container.RegisterType<IPacketProcessorActions, PacketProcessActions>()
                .RegisterType<INetworkConnectionActions, NetworkConnectionActions>()
                .RegisterType<IPacketHandlingActions, PacketHandlingActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>();
            
            //must be a singleton: tracks a thread and has internal state.
            //todo: see if this can be re-worked so the actions are stateless
            container.RegisterInstance<IBackgroundReceiveActions, BackgroundReceiveActions>();

            container.RegisterType<IChatPacketBuilder, ChatPacketBuilder>();

            //packet handling
            container.RegisterInstance<IOutOfBandPacketHandler, OutOfBandPacketHandler>()
                .RegisterType<IPacketHandlerFinder, PacketHandlerFinder>();

            container.RegisterVaried<IPacketHandler, ConnectionPlayerHandler>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //create the client object (can be recreated later)
            //todo: make this constructor injected in INetworkClientRepository?
            var clientRepo = container.Resolve<INetworkClientRepository>();
            var clientFactory = container.Resolve<INetworkClientFactory>();
            clientRepo.NetworkClient = clientFactory.CreateNetworkClient();
        }
    }
}
