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

        void LoadDataFiles();

        void LoadConfigFile();
    }
}
