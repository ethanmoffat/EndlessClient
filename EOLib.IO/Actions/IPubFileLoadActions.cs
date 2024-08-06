using System.Collections.Generic;

namespace EOLib.IO.Actions
{
    public interface IPubFileLoadActions
    {
        void LoadItemFile();

        void LoadItemFileByName(string fileName);

        void LoadNPCFile();

        void LoadNPCFileByName(string fileName);

        void LoadSpellFile();

        void LoadSpellFileByName(string fileName);

        void LoadClassFile();

        void LoadClassFileByName(string fileName);
    }
}
