using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using AutomaticTypeMapper;
using EndlessClient.Initialization;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.GameExecution
{
    public abstract class GameRunnerBase : IGameRunner
    {
        private readonly ITypeRegistry _registry;
        private readonly string[] _args;

        protected GameRunnerBase(ITypeRegistry registry, string[] args)
        {
            _registry = registry;
            _args = args;
        }

        public virtual bool SetupDependencies()
        {
            _registry.RegisterDiscoveredTypes();

            var initializers = _registry.ResolveAll<IGameInitializer>();
            try
            {
                foreach (var initializer in initializers)
                {
                    initializer.Initialize();
                }
            }
            catch (ConfigLoadException cle)
            {
                ShowErrorMessage(cle.Message, "Error loading config file!");
                return false;
            }
            catch (DataFileLoadException dfle)
            {
                ShowErrorMessage(dfle.Message, "Error loading data files!");
                return false;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                ShowErrorMessage(dnfe.Message, "Missing required directory");
                return false;
            }
            catch (FileNotFoundException fnfe)
            {
                ShowErrorMessage(fnfe.Message, "Missing required file");
                return false;
            }
            catch (LibraryLoadException lle)
            {
                var message =
                    $"There was an error loading GFX{(int) lle.WhichGFX:000}.EGF : {lle.WhichGFX}. Place all .GFX files in .\\gfx\\. The error message is:\n\n\"{lle.Message}\"";
                ShowErrorMessage(message, "GFX Load Error");
                return false;
            }

            for (int i = 0; i < _args.Length; ++i)
            {
                var arg = _args[i];

                if (string.Equals(arg, "--host") && i < _args.Length - 1)
                {
                    var host = _args[i + 1];
                    _registry.Resolve<IConfigurationRepository>()
                        .Host = host;

                    i++;
                }
                else
                {
                    Debug.WriteLine($"Unrecognized argument: {arg}. Will be ignored.");
                }
            }

            return true;
        }

        private void ShowErrorMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public virtual void RunGame()
        {
            var game = _registry.Resolve<IEndlessGame>();
            game.Run();
        }
    }
}
