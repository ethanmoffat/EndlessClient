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

            Assert.That(_configurationRepository.VersionMajor, Is.EqualTo(ConfigDefaults.MajorVersion));
            Assert.That(_configurationRepository.VersionMinor, Is.EqualTo(ConfigDefaults.MinorVersion));
            Assert.That(_configurationRepository.VersionBuild, Is.EqualTo(ConfigDefaults.ClientVersion));

            Assert.That(_configurationRepository.Host, Is.EqualTo(ConfigDefaults.Host));
            Assert.That(_configurationRepository.Port, Is.EqualTo(ConfigDefaults.Port));

            Assert.That(_configurationRepository.Language, Is.EqualTo(EOLanguage.English));
            Assert.That(_configurationRepository.CurseFilterEnabled, Is.False);
            Assert.That(_configurationRepository.StrictFilterEnabled, Is.False);

            Assert.That(_configurationRepository.ShowShadows, Is.True);
            Assert.That(_configurationRepository.ShowChatBubbles, Is.True);
            Assert.That(_configurationRepository.ShowTransition, Is.False);

            Assert.That(_configurationRepository.MusicEnabled, Is.False);
            Assert.That(_configurationRepository.SoundEnabled, Is.False);

            Assert.That(_configurationRepository.HearWhispers, Is.True);
            Assert.That(_configurationRepository.Interaction, Is.True);
            Assert.That(_configurationRepository.LogChatToFile, Is.False);
        }

        [Test]
        public void ValidConfigFile_LoadsSpecifiedSettings()
        {
            const string contents = @"[CONNECTION]
Host=eoserv.moffat.io
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

            Assert.That(_configurationRepository.VersionMajor, Is.EqualTo(10));
            Assert.That(_configurationRepository.VersionMinor, Is.EqualTo(20));
            Assert.That(_configurationRepository.VersionBuild, Is.EqualTo(30));

            Assert.That(_configurationRepository.Host, Is.EqualTo("eoserv.moffat.io"));
            Assert.That(_configurationRepository.Port, Is.EqualTo(12345));

            Assert.That(_configurationRepository.Language, Is.EqualTo(EOLanguage.Swedish));
            Assert.That(_configurationRepository.CurseFilterEnabled, Is.True);
            Assert.That(_configurationRepository.StrictFilterEnabled, Is.True);

            Assert.That(_configurationRepository.ShowShadows, Is.False);
            Assert.That(_configurationRepository.ShowChatBubbles, Is.False);
            Assert.That(_configurationRepository.ShowTransition, Is.True);
            Assert.That(_configurationRepository.InGameWidth, Is.EqualTo(123));
            Assert.That(_configurationRepository.InGameHeight, Is.EqualTo(321));

            Assert.That(_configurationRepository.MusicEnabled, Is.True);
            Assert.That(_configurationRepository.SoundEnabled, Is.True);

            Assert.That(_configurationRepository.HearWhispers, Is.False);
            Assert.That(_configurationRepository.Interaction, Is.False);
            Assert.That(_configurationRepository.LogChatToFile, Is.True);
        }

        private static void CreateTestConfigurationInDirectory(string contents)
        {
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);

            File.WriteAllText(ConfigStrings.Default_Config_File, contents);
        }
    }
}
