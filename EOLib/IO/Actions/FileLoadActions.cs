// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.IO.Actions
{
	public class FileLoadActions : IFileLoadActions
	{
		private readonly IPubFileRepository _pubFileRepository;
		private readonly IMapFileRepository _mapFileRepository;
		private readonly IDataFileRepository _dataFileRepository;
		private readonly IConfigurationRepository _configRepository;
		private readonly IPubLoadService<ItemRecord> _itemFileLoadService;
		private readonly IPubLoadService<NPCRecord> _npcFileLoadService;
		private readonly IPubLoadService<SpellRecord> _spellFileLoadService;
		private readonly IPubLoadService<ClassRecord> _classFileLoadService;
		private readonly IMapFileLoadService _mapFileLoadService;

		public FileLoadActions(IPubFileRepository pubFileRepository,
							   IMapFileRepository mapFileRepository,
							   IDataFileRepository dataFileRepository,
							   IConfigurationRepository configRepository,
							   IPubLoadService<ItemRecord> itemFileLoadService,
							   IPubLoadService<NPCRecord> npcFileLoadService,
							   IPubLoadService<SpellRecord> spellFileLoadService,
							   IPubLoadService<ClassRecord> classFileLoadService,
							   IMapFileLoadService mapFileLoadService)
		{
			_pubFileRepository = pubFileRepository;
			_mapFileRepository = mapFileRepository;
			_dataFileRepository = dataFileRepository;
			_configRepository = configRepository;
			_itemFileLoadService = itemFileLoadService;
			_npcFileLoadService = npcFileLoadService;
			_spellFileLoadService = spellFileLoadService;
			_classFileLoadService = classFileLoadService;
			_mapFileLoadService = mapFileLoadService;
		}

		public void LoadItemFile()
		{
			LoadItemFileByName(Constants.ItemFilePath);
		}

		public void LoadItemFileByName(string fileName)
		{
			var itemFile = _itemFileLoadService.LoadPubFromExplicitFile(fileName);
			_pubFileRepository.ItemFile = itemFile;
		}

		public void LoadNPCFile()
		{
			LoadNPCFileByName(Constants.NPCFilePath);
		}

		public void LoadNPCFileByName(string fileName)
		{
			var npcFile = _npcFileLoadService.LoadPubFromExplicitFile(fileName);
			_pubFileRepository.NPCFile = npcFile;
		}

		public void LoadSpellFile()
		{
			LoadSpellFileByName(Constants.SpellFilePath);
		}

		public void LoadSpellFileByName(string fileName)
		{
			var spellFile = _spellFileLoadService.LoadPubFromExplicitFile(fileName);
			_pubFileRepository.SpellFile = spellFile;
		}

		public void LoadClassFile()
		{
			LoadClassFileByName(Constants.ClassFilePath);
		}

		public void LoadClassFileByName(string fileName)
		{
			var classFile = _classFileLoadService.LoadPubFromExplicitFile(fileName);
			_pubFileRepository.ClassFile = classFile;
		}

		public void LoadMapFileByID(int id)
		{
			LoadMapFileByName(string.Format(Constants.MapFileFormatString, id));
		}

		public void LoadMapFileByName(string fileName)
		{
			var mapFile = _mapFileLoadService.LoadMapByPath(fileName);
			if (_mapFileRepository.MapFiles.ContainsKey(mapFile.Properties.MapID))
				_mapFileRepository.MapFiles[mapFile.Properties.MapID] = mapFile;
			else
				_mapFileRepository.MapFiles.Add(mapFile.Properties.MapID, mapFile);
		}

		//todo: LoadDataFiles and LoadConfigFile should probably have some logic extracted into services

		public void LoadDataFiles()
		{
			var files = Directory.GetFiles(Constants.DataFilePath, "*.edf");

			if (!Directory.Exists(Constants.DataFilePath) ||
			    files.Length != Constants.ExpectedNumberOfDataFiles)
				throw new DataFileLoadException();

			_dataFileRepository.DataFiles.Clear();
			for (int i = 1; i < Constants.ExpectedNumberOfDataFiles; ++i)
			{
				var fileToLoad = (DataFiles) i;
				var loadedFile = new EDFFile(files[i], fileToLoad);
				
				_dataFileRepository.DataFiles.Add(fileToLoad, loadedFile);
			}
		}

		public void LoadConfigFile()
		{
			var configFile = new IniReader(ConfigStrings.Default_Config_File);

			int tempInt;
			_configRepository.VersionMajor = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Major, out tempInt)
				? (byte) tempInt
				: Constants.MajorVersion;

			_configRepository.VersionMinor = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Minor, out tempInt)
				? (byte) tempInt
				: Constants.MinorVersion;

			_configRepository.VersionBuild = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Client, out tempInt)
				? (byte) tempInt
				: Constants.ClientVersion;

			_configRepository.Language = configFile.GetValue(ConfigStrings.LANGUAGE, ConfigStrings.Language, out tempInt)
				? (EOLanguage) tempInt
				: EOLanguage.English;

			_configRepository.PlayerDropProtectTime = configFile.GetValue(ConfigStrings.Custom,
				ConfigStrings.PlayerDropProtectTime, out tempInt)
				? tempInt
				: Constants.PlayerDropProtectionSeconds;

			_configRepository.NPCDropProtectTime = configFile.GetValue(ConfigStrings.Custom, ConfigStrings.NPCDropProtectTime,
				out tempInt)
				? tempInt
				: Constants.NPCDropProtectionSeconds;

			bool tempBool;
			_configRepository.CurseFilterEnabled = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.Filter, out tempBool) && tempBool;
			_configRepository.StrictFilterEnabled = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.FilterAll, out tempBool) && tempBool;

			_configRepository.ShowShadows = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowShadows, out tempBool) || tempBool;
			_configRepository.ShowTransition = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowTransition, out tempBool) || tempBool;
			_configRepository.MusicEnabled = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.Music, out tempBool) && tempBool;
			_configRepository.SoundEnabled = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.Sound, out tempBool) && tempBool;
			_configRepository.ShowChatBubbles = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowBaloons, out tempBool) || tempBool;

			_configRepository.EnableLog = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.EnableLogging, out tempBool) && tempBool;
			_configRepository.HearWhispers = !configFile.GetValue(ConfigStrings.Chat, ConfigStrings.HearWhisper, out tempBool) || tempBool;
			_configRepository.Interaction = !configFile.GetValue(ConfigStrings.Chat, ConfigStrings.Interaction, out tempBool) || tempBool;
			_configRepository.LogChatToFile = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.LogChat, out tempBool) && tempBool;

			string host;
			_configRepository.Host = configFile.GetValue(ConfigStrings.Connection, ConfigStrings.Host, out host) ? host : Constants.Host;
			_configRepository.Port = configFile.GetValue(ConfigStrings.Connection, ConfigStrings.Port, out tempInt) ? tempInt : Constants.Port;
		}
	}
}
