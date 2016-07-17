// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using Microsoft.Practices.Unity;

namespace EOLib.Net.Translators
{
    public class PacketTranslatorContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IPacketTranslator<IInitializationData>, InitDataTranslator>();
            container.RegisterType<IPacketTranslator<IAccountLoginData>, AccountLoginPacketTranslator>();
            container.RegisterType<IPacketTranslator<ICharacterCreateData>, CharacterReplyPacketTranslator>();
            container.RegisterType<IPacketTranslator<ILoginRequestGrantedData>, LoginRequestGrantedPacketTranslator>();
            container.RegisterType<IPacketTranslator<ILoginRequestCompletedData>, LoginRequestCompletedPacketTranslator>();
        }
    }
}
