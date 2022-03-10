using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public interface IMapFileSaveService
    {
        void SaveFileToDefaultDirectory(IMapFile mapFile, bool rewriteChecksum = true);

        void SaveFile(string path, IMapFile pubFile, bool rewriteChecksum = true);
    }
}
