// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO
{
	internal class NPCRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new NPCRecord(id);
		}
	}
}
