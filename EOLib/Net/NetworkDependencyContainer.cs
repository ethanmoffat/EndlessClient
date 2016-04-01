// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;
using Microsoft.Practices.Unity;

namespace EOLib.Net
{
	public class NetworkDependencyContainer : IDependencyContainer, IInitializableContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<IPacketEncoderService, PacketEncoderService>();
			container.RegisterType<IPacketSequenceService, PacketSequenceService>();
			container.RegisterType<IInitDataGeneratorService, InitDataGeneratorService>();
			container.RegisterType<IHashService, HashService>();

			container.RegisterType<INetworkClientFactory, NetworkClientFactory>();

			container.RegisterType<INetworkClientRepository, NetworkClientRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<INetworkClientProvider, NetworkClientRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPacketQueue, PacketQueue>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPacketEncoderRepository, PacketEncoderRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ISequenceRepository, SequenceRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<IPacketProcessorActions, PacketProcessActions>();
			container.RegisterType<INetworkConnectionActions, NetworkConnectionActions>();
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			var clientRepo = container.Resolve<INetworkClientRepository>();
			var clientFactory = container.Resolve<INetworkClientFactory>();

			clientRepo.NetworkClient = clientFactory.CreateNetworkClient();
		}
	}
}
