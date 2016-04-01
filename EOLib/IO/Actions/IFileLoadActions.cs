// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Actions
{
	public interface IFileLoadActions
	{
		void LoadItemFile();

		void LoadItemFileByName(string fileName);

		void LoadNPCFile();

		void LoadNPCFileByName(string fileName);

		void LoadSpellFile();

		void LoadSpellFileByName(string fileName);

		void LoadClassFile();

		void LoadClassFileByName(string fileName);

		void LoadMapFileByID(int id);

		void LoadMapFileByName(string fileName);
	}

	//todo list:
	//2. implementation for file load services
	//3. .EDF repositories/services/load actions
	//4. config load actions
	//5. move IPacketQueue from NetworkClient-> repository
}
