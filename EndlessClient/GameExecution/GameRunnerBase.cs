// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using System.Windows.Forms;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using Microsoft.Practices.Unity;

namespace EndlessClient.GameExecution
{
	public abstract class GameRunnerBase : IGameRunner
	{
		private readonly IUnityContainer _unityContainer;

		protected GameRunnerBase(IUnityContainer unityContainer)
		{
			_unityContainer = unityContainer;
		}

		public virtual bool SetupDependencies()
		{
			var registrar = new DependencyRegistrar(_unityContainer);

			registrar.RegisterDependencies(DependencyContainerProvider.DependencyContainers);

			try
			{
				registrar.InitializeDependencies(
					DependencyContainerProvider.DependencyContainers
						.OfType<IInitializableContainer>()
						.ToArray());
			}
			catch (ConfigLoadException cle)
			{
				MessageBox.Show(cle.Message,
					"Error loading config file!",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return false;
			}
			catch (DataFileLoadException dfle)
			{
				MessageBox.Show(dfle.Message,
					"Error loading data files!",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return false;
			}
			catch (LibraryLoadException lle)
			{
				MessageBox.Show(
					string.Format(
						"There was an error loading GFX{0:000}.EGF : {1}. Place all .GFX files in .\\gfx\\. The error message is:\n\n\"{2}\"",
						(int) lle.WhichGFX,
						lle.WhichGFX,
						lle.Message),
					"GFX Load Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		public virtual void RunGame()
		{
			var game = _unityContainer.Resolve<IEndlessGame>();
			game.Run();
		}
	}
}
