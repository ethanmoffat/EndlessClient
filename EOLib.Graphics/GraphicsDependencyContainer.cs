// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using Microsoft.Practices.Unity;
using PELoaderLib;

namespace EOLib.Graphics
{
	public class GraphicsDependencyContainer : IInitializableContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterInstance<INativeGraphicsManager, NativeGraphicsManager>();
			container.RegisterInstance<INativeGraphicsLoader, NativeGraphicsLoader>();

			container.RegisterInstance<IGraphicsDeviceRepository, GraphicsDeviceRepository>();
			container.RegisterInstance<IGraphicsDeviceProvider, GraphicsDeviceRepository>();
			
			container.RegisterInstance<IPEFileCollection, PEFileCollection>();
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			var files = container.Resolve<IPEFileCollection>();

			foreach (var filePair in files)
				TryInitializePEFiles(filePair.Key, filePair.Value);
		}

		private static void TryInitializePEFiles(GFXTypes file, IPEFile peFile)
		{
			var number = ((int) file).ToString("D3");

			try
			{
				peFile.Initialize();
			}
			catch (IOException)
			{
				throw new LibraryLoadException(number, file);
			}

			if (!peFile.Initialized)
				throw new LibraryLoadException(number, file);
		}
	}
}
