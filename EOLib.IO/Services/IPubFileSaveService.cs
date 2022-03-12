using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public interface IPubFileSaveService
    {
        void SaveFile(string path, IPubFile pubFile, bool rewriteChecksum = true);
    }
}
