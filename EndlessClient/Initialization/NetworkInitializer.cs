using AutomaticTypeMapper;
using EOLib.Net.Communication;

namespace EndlessClient.Initialization
{
    [AutoMappedType]
    public class NetworkInitializer : IGameInitializer
    {
        private readonly INetworkClientRepository _networkClientRepository;
        private readonly INetworkClientFactory _networkClientFactory;

        public NetworkInitializer(INetworkClientRepository networkClientRepository,
                                  INetworkClientFactory networkClientFactory)
        {
            _networkClientRepository = networkClientRepository;
            _networkClientFactory = networkClientFactory;
        }

        public void Initialize()
        {
            //create the client object (can be recreated later)
            //constructor-injecting the factory causes a dependency loop.
            _networkClientRepository.NetworkClient = _networkClientFactory.CreateNetworkClient();
        }
    }
}
