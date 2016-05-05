// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EOLib.Domain;
using EOLib.IO.Repositories;
using EOLib.Net.API;

namespace EOLib.IO.Actions
{
	public class FileRequestActions : IFileRequestActions
	{
		private readonly INumberEncoderService _numberEncoderService;
		private readonly ILoginFileChecksumProvider _loginFileChecksumProvider;
		private readonly IPubFileRepository _pubFileRepository;
		private readonly IMapFileRepository _mapFileRepository;

		public FileRequestActions(INumberEncoderService numberEncoderService,
								  ILoginFileChecksumProvider loginFileChecksumProvider,
								  IPubFileRepository pubFileRepository,
								  IMapFileRepository mapFileRepository)
		{
			_numberEncoderService = numberEncoderService;
			_loginFileChecksumProvider = loginFileChecksumProvider;
			_pubFileRepository = pubFileRepository;
			_mapFileRepository = mapFileRepository;
		}

		public bool NeedsFile(InitFileType fileType, short optionalID = 0)
		{
			if (fileType == InitFileType.Map)
				return NeedMap(optionalID);
			
			return NeedPub(fileType);
		}

		public async Task GetMapFromServer(short mapID)
		{
			await Task.FromResult(false);
		}

		public async Task GetItemFileFromServer()
		{
			await Task.FromResult(false);
		}

		public async Task GetNPCFileFromServer()
		{
			await Task.FromResult(false);
		}

		public async Task GetSpellFileFromServer()
		{
			await Task.FromResult(false);
		}

		public async Task GetClassFileFromServer()
		{
			await Task.FromResult(false);
		}

		private bool NeedMap(short mapID)
		{
			try
			{
				var expectedChecksum = _numberEncoderService.DecodeNumber(_loginFileChecksumProvider.MapChecksum);
				var expectedLength = _loginFileChecksumProvider.MapLength;

				var actualChecksum = _numberEncoderService.DecodeNumber(_mapFileRepository.MapFiles[mapID].Properties.Checksum);
				var actualLength = _mapFileRepository.MapFiles[mapID].Properties.FileSize;

				return expectedChecksum != actualChecksum || expectedLength != actualLength;
			}
			catch (KeyNotFoundException) { return true; } //map id is not in the map file repository
		}

		private bool NeedPub(InitFileType fileType)
		{
			switch (fileType)
			{
				case InitFileType.Item:
					return _pubFileRepository.ItemFile == null ||
					       _loginFileChecksumProvider.EIFChecksum != _pubFileRepository.ItemFile.Rid ||
					       _loginFileChecksumProvider.EIFLength != _pubFileRepository.ItemFile.Len;
				case InitFileType.Npc:
					return _pubFileRepository.NPCFile == null ||
					       _loginFileChecksumProvider.ENFChecksum != _pubFileRepository.NPCFile.Rid ||
					       _loginFileChecksumProvider.ENFLength != _pubFileRepository.NPCFile.Len;
				case InitFileType.Spell:
					return _pubFileRepository.SpellFile == null ||
					       _loginFileChecksumProvider.ESFChecksum != _pubFileRepository.SpellFile.Rid ||
					       _loginFileChecksumProvider.ESFLength != _pubFileRepository.SpellFile.Len;
				case InitFileType.Class:
					return _pubFileRepository.ClassFile == null ||
					       _loginFileChecksumProvider.ECFChecksum != _pubFileRepository.ClassFile.Rid ||
					       _loginFileChecksumProvider.ECFLength != _pubFileRepository.ClassFile.Len;
				default:
					throw new ArgumentOutOfRangeException("fileType", fileType, null);
			}
		}
	}
}