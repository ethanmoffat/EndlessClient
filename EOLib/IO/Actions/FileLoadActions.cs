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
		private readonly IPubLoadService<ItemRecord> _itemFileLoadService;
		private readonly IPubLoadService<NPCRecord> _npcFileLoadService;
		private readonly IPubLoadService<SpellRecord> _spellFileLoadService;
		private readonly IPubLoadService<ClassRecord> _classFileLoadService;
		private readonly IMapFileLoadService _mapFileLoadService;

		public FileLoadActions(IPubFileRepository pubFileRepository,
							   IMapFileRepository mapFileRepository,
							   IDataFileRepository dataFileRepository,
							   IPubLoadService<ItemRecord> itemFileLoadService,
							   IPubLoadService<NPCRecord> npcFileLoadService,
							   IPubLoadService<SpellRecord> spellFileLoadService,
							   IPubLoadService<ClassRecord> classFileLoadService,
							   IMapFileLoadService mapFileLoadService)
		{
			_pubFileRepository = pubFileRepository;
			_mapFileRepository = mapFileRepository;
			_dataFileRepository = dataFileRepository;
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
	}
}
