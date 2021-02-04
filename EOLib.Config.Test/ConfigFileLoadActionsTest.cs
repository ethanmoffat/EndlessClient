using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace EOLib.Config.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ConfigFileLoadActionsTest
    {
        private const string ConfigDirectory = "config";

        private IConfigFileLoadActions _configFileLoadActions;
        private IConfigurationRepository _configurationRepository;

        [SetUp]
        public void SetUp()
        {
            _configurationRepository = new ConfigurationRepository();
            _configFileLoadActions = new ConfigFileLoadActions(_configurationRepository);
        }

        [TearDown]
        public static void TearDown()
        {
            if (Directory.Exists(ConfigDirectory))
                Directory.Delete(ConfigDirectory, true);
        }

        [Test]
        public void MissingConfigurationFile_ThrowsConfigLoadException()
        {
            Assert.Throws<ConfigLoadException>(() => _configFileLoadActions.LoadConfigFile());
        }

        [Test]
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

            Assert.AreEqual(EOLanguage.English, _configurationRepository.Language);
            Assert.IsFalse(_configurationRepository.CurseFilterEnabled);
            Assert.IsFalse(_configurationRepository.StrictFilterEnabled);

            Assert.IsTrue(_configurationRepository.ShowShadows);
            Assert.IsTrue(_configurationRepository.ShowChatBubbles);
            Assert.IsFalse(_configurationRepository.ShowTransition);

            Assert.AreEqual(ConfigDefaults.InGameWidth, _configurationRepository.InGameWidth);
            Assert.AreEqual(ConfigDefaults.InGameHeight, _configurationRepository.InGameHeight);

            Assert.IsFalse(_configurationRepository.MusicEnabled);
            Assert.IsFalse(_configurationRepository.SoundEnabled);
            
            Assert.IsTrue(_configurationRepository.HearWhispers);
            Assert.IsTrue(_configurationRepository.Interaction);
            Assert.IsFalse(_configurationRepository.LogChatToFile);
            Assert.IsFalse(_configurationRepository.EnableLog);
        }

        [Test]
        public void ValidConfigFile_LoadsSpecifiedSettings()
        {
            const string contents = @"[CONNECTION]
Host=ewmoffat.ddns.net
Port=12345
[VERSION]
Major=10
Minor=20
Client=30
[SETTINGS]
Music=on
Sound=on
ShowBaloons=off
ShowShadows=no
ShowTransition=true
EnableLogging=true
InGameWidth=123
InGameHeight=321
[CUSTOM]
NPCDropProtectTime=5000
PlayerDropProtectTime=10000
[LANGUAGE]
Language=2
[CHAT]
Filter=on
FilterAll=on
LogChat=on
LogFile=CHATLOG.TXT
HearWhisper=off
Interaction=false";
            CreateTestConfigurationInDirectory(contents);

            _configFileLoadActions.LoadConfigFile();

            Assert.AreEqual(10, _configurationRepository.VersionMajor);
            Assert.AreEqual(20, _configurationRepository.VersionMinor);
            Assert.AreEqual(30, _configurationRepository.VersionBuild);

            Assert.AreEqual("ewmoffat.ddns.net", _configurationRepository.Host);
            Assert.AreEqual(12345, _configurationRepository.Port);

            Assert.AreEqual(5000, _configurationRepository.NPCDropProtectTime);
            Assert.AreEqual(10000, _configurationRepository.PlayerDropProtectTime);

            Assert.AreEqual(EOLanguage.Swedish, _configurationRepository.Language);
            Assert.IsTrue(_configurationRepository.CurseFilterEnabled);
            Assert.IsTrue(_configurationRepository.StrictFilterEnabled);

            Assert.IsFalse(_configurationRepository.ShowShadows);
            Assert.IsFalse(_configurationRepository.ShowChatBubbles);
            Assert.IsTrue(_configurationRepository.ShowTransition);
            Assert.AreEqual(123, _configurationRepository.InGameWidth);
            Assert.AreEqual(321, _configurationRepository.InGameHeight);

            Assert.IsTrue(_configurationRepository.MusicEnabled);
            Assert.IsTrue(_configurationRepository.SoundEnabled);

            Assert.IsFalse(_configurationRepository.HearWhispers);
            Assert.IsFalse(_configurationRepository.Interaction);
            Assert.IsTrue(_configurationRepository.LogChatToFile);
            Assert.IsTrue(_configurationRepository.EnableLog);
        }

        private static void CreateTestConfigurationInDirectory(string contents)
        {
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);

            File.WriteAllText(ConfigStrings.Default_Config_File, contents);
        }
    }
}