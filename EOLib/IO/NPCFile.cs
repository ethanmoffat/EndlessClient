// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace EOLib.IO
{
	public class NPCFile : EODataFile
	{
		public const int DATA_SIZE = 39;

		public NPCFile()
			: base(new NPCRecordFactory())
		{
			Load(FilePath = Constants.NPCFilePath);
		}

		public NPCFile(string path)
			: base(new NPCRecordFactory())
		{
			Load(FilePath = path);
		}

		protected override int GetDataSize()
		{
			return DATA_SIZE;
		}

		public NPCRecord GetNPCRecordByID(short id)
		{
			return Data.OfType<NPCRecord>().Single(x => x.ID == id);
		}
	}
}
