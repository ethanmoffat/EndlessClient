using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Shared;

namespace EndlessClient.Initialization
{
    [MappedType(BaseType = typeof(IGameInitializer))]
    public class ConfigInitializer : IGameInitializer
    {
        private readonly IConfigFileLoadActions _configFileLoadActions;

        public ConfigInitializer(IConfigFileLoadActions configFileLoadActions)
        {
            _configFileLoadActions = configFileLoadActions;
        }

        public void Initialize()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                InitializeDefaultConfigFiles();

            _configFileLoadActions.LoadConfigFile();
        }

        /// <summary>
        /// Copy default configuration files out of the macOS app bundle into ~/.endlessclient/config. This is done only if the files do not already exist.
        /// </summary>
        [SupportedOSPlatform("OSX")]
        private static void InitializeDefaultConfigFiles()
        {
            var configDirectory = Path.GetDirectoryName(Constants.Default_Config_File);
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            var files = new[] {
                Constants.Default_Config_File,
                Constants.InventoryFile,
                Constants.SpellsFile,
                Constants.FriendListFile,
                Constants.IgnoreListFile,
                Constants.PanelLayoutFile
            };
            foreach (var file in files)
            {
                if (!File.Exists(file) && file.Contains(PathResolver.LocalFilesRoot))
                {
                    var index = file.IndexOf(PathResolver.LocalFilesRoot) + PathResolver.LocalFilesRoot.Length + 1;
                    var source = Path.Combine(PathResolver.ResourcesRoot, file[index..]);
                    if (File.Exists(source))
                    {
                        File.Copy(source, file);
                    }
                }
            }
        }
    }
}
