// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Microsoft.Practices.Unity;

namespace EOLib.Domain
{
    public class DataDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<ICreateAccountParameterValidator, CreateAccountParameterValidator>()
                .RegisterType<IChatModeCalculatorService, ChatModeCalculatorService>();

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
                .RegisterInstance<IChatTextRepository, ChatTextRepository>()
                .RegisterInstance<IChatTextProvider, ChatTextRepository>();
        }
    }
}
