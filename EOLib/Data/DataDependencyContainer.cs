// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Data.AccountCreation;
using Microsoft.Practices.Unity;

namespace EOLib.Data
{
	public class DataDependencyContainer : IDependencyContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<INumberEncoderService, NumberEncoderService>();
			container.RegisterType<ICreateAccountParameterValidator, CreateAccountParameterValidator>();

			container.RegisterType<IAccountActions, AccountActions>();
		}
	}
}
