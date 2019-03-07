// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using Unity;

namespace EOLib.Domain.Notifiers
{
    //The default registrations for the no-op types prevent Unity resolution errors
    //  at runtime in the case where an external application wants to use EOLib
    public class NotifierDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterVaried<IChatEventNotifier, NoOpChatEventNotifier>()
                .RegisterVaried<IMapChangedNotifier, NoOpMapChangedNotifier>()
                .RegisterVaried<INPCActionNotifier, NoOpNPCActionNotifier>()
                .RegisterVaried<IOtherCharacterAnimationNotifier, NoOpOtherCharacterAnimationNotifier>()
                .RegisterVaried<IMainCharacterEventNotifier, NoOpMainCharacterEventNotifier>()
                .RegisterVaried<IOtherCharacterEventNotifier, NoOpOtherCharacterEventNotifier>();
        }
    }
}
