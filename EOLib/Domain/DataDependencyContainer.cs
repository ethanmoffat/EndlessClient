// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Account;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
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
			container.RegisterType<ILoggedInAccountNameRepository, LoggedInAccountNameRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ILoggedInAccountNameProvider, LoggedInAccountNameRepository>(new ContainerControlledLifetimeManager());
		}
	}
}
