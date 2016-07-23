// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Microsoft.Practices.Unity;

namespace EOLib.Domain
{
    public class DataDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<ICreateAccountParameterValidator, CreateAccountParameterValidator>();

            container.RegisterType<IAccountActions, AccountActions>();
            container.RegisterType<ILoginActions, LoginActions>();
            container.RegisterType<ICharacterManagementActions, CharacterManagementActions>();

            container.RegisterInstance<ICharacterSelectorRepository, CharacterSelectorRepository>();
            container.RegisterInstance<ICharacterSelectorProvider, CharacterSelectorRepository>();
            container.RegisterInstance<IPlayerInfoRepository, PlayerInfoRepository>();
            container.RegisterInstance<IPlayerInfoProvider, PlayerInfoRepository>();
            container.RegisterInstance<ICharacterRepository, CharacterRepository>();
            container.RegisterInstance<ICharacterProvider, CharacterRepository>();
            container.RegisterInstance<ICurrentMapStateRepository, CurrentMapStateRepository>();
            container.RegisterInstance<ICurrentMapStateProvider, CurrentMapStateRepository>();
            container.RegisterInstance<ICharacterInventoryRepository, CharacterInventoryRepository>();
            container.RegisterInstance<ICharacterInventoryProvider, CharacterInventoryRepository>();
            container.RegisterInstance<INewsRepository, NewsRepository>();
            container.RegisterInstance<INewsProvider, NewsRepository>();
        }
    }
}
