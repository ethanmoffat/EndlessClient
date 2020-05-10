using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public interface IMapFileSaveService
    {
        void SaveFileToDefaultDirectory(IMapFile mapFile);

        void SaveFile(string path, IMapFile pubFile);
    }
}
