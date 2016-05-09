// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Practices.Unity;

namespace EOLib.Graphics
{
	public class GraphicsDependencyContainer : IDependencyContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterInstance<INativeGraphicsManager, NativeGraphicsManager>();
			container.RegisterInstance<INativeGraphicsLoader, NativeGraphicsLoader>();

			container.RegisterInstance<IGraphicsDeviceRepository, GraphicsDeviceRepository>();
			container.RegisterInstance<IGraphicsDeviceProvider, GraphicsDeviceRepository>();
		}
	}
}
