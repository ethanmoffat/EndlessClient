// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Practices.Unity;

namespace EOLib
{
	public interface IInitializableContainer : IDependencyContainer
	{
		void InitializeDependencies(IUnityContainer container);
	}
}
