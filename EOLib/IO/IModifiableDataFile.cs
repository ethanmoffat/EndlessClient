// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO
{
	public interface IModifiableDataFile<T> : IDataFile<T>
		where T : IDataRecord
	{
		void Save(string fileName, int version = 0);

		void ReplaceRecordAt(int index, T newElement);
	}
}
