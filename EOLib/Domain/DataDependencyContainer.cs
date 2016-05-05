// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
			container.RegisterType<INumberEncoderService, NumberEncoderService>();
			container.RegisterType<ICreateAccountParameterValidator, CreateAccountParameterValidator>();

			container.RegisterType<IAccountActions, AccountActions>();
			container.RegisterType<ILoginActions, LoginActions>();
			container.RegisterType<ICharacterManagementActions, CharacterManagementActions>();

			container.RegisterType<ICharacterSelectorRepository, CharacterSelectorRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterSelectorProvider, CharacterSelectorRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPlayerInfoRepository, PlayerInfoRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ILoggedInAccountNameProvider, PlayerInfoRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterRepository, CharacterRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterProvider, CharacterRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICurrentMapStateRepository, CurrentMapStateRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICurrentMapProvider, CurrentMapStateRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterInventoryRepository, CharacterInventoryRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterInventoryProvider, CharacterInventoryRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<INewsRepository, NewsRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<INewsProvider, NewsRepository>(new ContainerControlledLifetimeManager());
		}
	}
}
