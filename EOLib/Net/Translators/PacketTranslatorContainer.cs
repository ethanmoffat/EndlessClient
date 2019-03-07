// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using Unity;

namespace EOLib.Net.Translators
{
    public class PacketTranslatorContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<ICharacterFromPacketFactory, CharacterFromPacketFactory>();

            container.RegisterType<IPacketTranslator<IInitializationData>, InitDataTranslator>()
                .RegisterType<IPacketTranslator<IAccountLoginData>, AccountLoginPacketTranslator>()
                .RegisterType<IPacketTranslator<ICharacterCreateData>, CharacterReplyPacketTranslator>()
                .RegisterType<IPacketTranslator<ILoginRequestGrantedData>, LoginRequestGrantedPacketTranslator>()
                .RegisterType<IPacketTranslator<ILoginRequestCompletedData>, LoginRequestCompletedPacketTranslator>()
                .RegisterType<IPacketTranslator<IWarpAgreePacketData>, WarpAgreePacketTranslator>()
                .RegisterType<IPacketTranslator<IRefreshReplyData>, RefreshReplyPacketTranslator>();
        }
    }
}
