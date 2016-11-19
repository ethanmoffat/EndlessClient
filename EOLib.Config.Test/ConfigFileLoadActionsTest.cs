// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.Config.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class ConfigFileLoadActionsTest
    {
        private const string ConfigDirectory = "config";

        private IConfigFileLoadActions _configFileLoadActions;
        private IConfigurationRepository _configurationRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _configurationRepository = new ConfigurationRepository();
            _configFileLoadActions = new ConfigFileLoadActions(_configurationRepository);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (Directory.Exists(ConfigDirectory))
                Directory.Delete(ConfigDirectory, true);
        }

        [TestMethod, ExpectedException(typeof(ConfigLoadException))]
        public void MissingConfigurationFile_ThrowsConfigLoadException()
        {
            _configFileLoadActions.LoadConfigFile();
        }

        [TestMethod]
        public void InvalidConfigFileThatExists_UsesConfigurationValueDefaults()
        {
            CreateTestConfigurationInDirectory("[Invalid]\nContents=heyayayayay");
            _configFileLoadActions.LoadConfigFile();

            Assert.AreEqual(ConfigDefaults.MajorVersion, _configurationRepository.VersionMajor);
            Assert.AreEqual(ConfigDefaults.MinorVersion, _configurationRepository.VersionMinor);
            Assert.AreEqual(ConfigDefaults.ClientVersion, _configurationRepository.VersionBuild);

            Assert.AreEqual(ConfigDefaults.Host, _configurationRepository.Host);
            Assert.AreEqual(ConfigDefaults.Port, _configurationRepository.Port);

            Assert.AreEqual(ConfigDefaults.NPCDropProtectionSeconds, _configurationRepository.NPCDropProtectTime);
            Assert.AreEqual(ConfigDefaults.PlayerDropProtectionSeconds, _configurationRepository.PlayerDropProtectTime);
        }

        private static void CreateTestConfigurationInDirectory(string contents)
        {
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);

            File.WriteAllText(ConfigStrings.Default_Config_File, contents);
        }
    }
}
