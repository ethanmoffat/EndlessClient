using System.Collections.Generic;

namespace EOLib.IO.Actions
{
    public interface IPubFileLoadActions
    {
        void LoadItemFile(IEnumerable<int> rangedWeaponIds);

        void LoadItemFileByName(string fileName, IEnumerable<int> rangedWeaponIds);

        void LoadNPCFile();

        void LoadNPCFileByName(string fileName);

        void LoadSpellFile();

        void LoadSpellFileByName(string fileName);

        void LoadClassFile();

        void LoadClassFileByName(string fileName);
    }
}
