// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.IO.Repositories
{
	public interface IFileChecksumRepository
	{
		int EIFChecksum { get; set; }

		int ENFChecksum { get; set; }

		int ESFChecksum { get; set; }

		int ECFChecksum { get; set; }

		short EIFLength { get; set; }

		short ENFLength { get; set; }

		short ESFLength { get; set; }

		short ECFLength { get; set; }

		Dictionary<short, byte[]> MapChecksums { get; set; }

		Dictionary<short, short> MapLengths { get; set; } 
	}

	public interface IFileChecksumProvider
	{
		int EIFChecksum { get; }

		int ENFChecksum { get; }

		int ESFChecksum { get; }

		int ECFChecksum { get; }

		short EIFLength { get; }

		short ENFLength { get; }

		short ESFLength { get; }

		short ECFLength { get; }

		Dictionary<short, byte[]> MapChecksums { get; }

		Dictionary<short, short> MapLengths { get; } 
	}

	public class FileChecksumRepository : IFileChecksumRepository, IFileChecksumProvider
	{
		public int EIFChecksum { get; set; }

		public int ENFChecksum { get; set; }

		public int ESFChecksum { get; set; }

		public int ECFChecksum { get; set; }

		public short EIFLength { get; set; }

		public short ENFLength { get; set; }

		public short ESFLength { get; set; }

		public short ECFLength { get; set; }

		public Dictionary<short, byte[]> MapChecksums { get; set; }

		public Dictionary<short, short> MapLengths { get; set; }

		public FileChecksumRepository()
		{
			MapChecksums = new Dictionary<short, byte[]>();
			MapLengths = new Dictionary<short, short>();
		}
	}
}
