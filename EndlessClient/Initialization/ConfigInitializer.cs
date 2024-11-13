using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AutomaticTypeMapper;
using EOLib;
using EOLib.Config;

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
            {
                CopyDefaultConfigFiles();
            }
            _configFileLoadActions.LoadConfigFile();
        }

        [SupportedOSPlatform("OSX")]
        private void CopyDefaultConfigFiles()
        {
            if (!Directory.Exists(Path.GetDirectoryName(ConfigStrings.Default_Config_File)))
            {
                Directory.CreateDirectory(ConfigStrings.Default_Config_File);
            }

            var files = new[] {
                ConfigStrings.Default_Config_File,
                Constants.InventoryFile,
                Constants.SpellsFile,
                Constants.FriendListFile,
                Constants.IgnoreListFile,
                Constants.PanelLayoutFile
            };
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    var index = file.IndexOf(".endlessclient") + 15;
                    var source = Path.Combine("Contents", "Resources", file[index..]);
                    if (File.Exists(source))
                    {
                        File.Copy(source, file);
                    }
                }
            }
        }
    }
}
