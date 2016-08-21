// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Chat.Commands;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using Microsoft.Practices.Unity;

namespace EOLib.Domain
{
    public class DomainDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<ICreateAccountParameterValidator, CreateAccountParameterValidator>();

            container.RegisterType<IAccountActions, AccountActions>()
                .RegisterType<ILoginActions, LoginActions>()
                .RegisterType<ICharacterManagementActions, CharacterManagementActions>();

            container.RegisterInstance<ICharacterSelectorRepository, CharacterSelectorRepository>()
                .RegisterInstance<ICharacterSelectorProvider, CharacterSelectorRepository>()
                .RegisterInstance<IPlayerInfoRepository, PlayerInfoRepository>()
                .RegisterInstance<IPlayerInfoProvider, PlayerInfoRepository>()
                .RegisterInstance<ICharacterRepository, CharacterRepository>()
                .RegisterInstance<ICharacterProvider, CharacterRepository>()
                .RegisterInstance<ICurrentMapStateRepository, CurrentMapStateRepository>()
                .RegisterInstance<ICurrentMapStateProvider, CurrentMapStateRepository>()
                .RegisterInstance<ICurrentMapProvider, CurrentMapProvider>()
                .RegisterInstance<ICharacterInventoryRepository, CharacterInventoryRepository>()
                .RegisterInstance<ICharacterInventoryProvider, CharacterInventoryRepository>()
                .RegisterInstance<IPaperdollRepository, PaperdollRepository>()
                .RegisterInstance<IPaperdollProvider, PaperdollRepository>()
                .RegisterInstance<INewsRepository, NewsRepository>()
                .RegisterInstance<INewsProvider, NewsRepository>()
                .RegisterInstance<IPingTimeRepository, PingTimeRepository>();

            RegisterTypesForChat(container);
        }

        private static void RegisterTypesForChat(IUnityContainer container)
        {
            container.RegisterInstance<IChatRepository, ChatRepository>()
                .RegisterInstance<IChatProvider, ChatRepository>()
                .RegisterType<IChatActions, ChatActions>()
                .RegisterType<ILocalCommandHandler, LocalCommandHandler>()
                .RegisterType<IChatTypeCalculator, ChatTypeCalculator>();

            container.RegisterVaried<IPlayerCommand, NoWallCommand>()
                .RegisterVaried<IPlayerCommand, LocCommand>()
                .RegisterVaried<IPlayerCommand, UsageCommand>()
                .RegisterVaried<IPlayerCommand, FindCommand>()
                .RegisterVaried<IPlayerCommand, PingCommand>();
        }
    }
}
