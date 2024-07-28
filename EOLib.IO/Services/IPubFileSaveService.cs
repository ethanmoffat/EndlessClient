using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public interface IPubFileSaveService
    {
        void SaveFile<TRecord>(string path, IPubFile<TRecord> pubFile, bool rewriteChecksum = true)
            where TRecord : class, IPubRecord, new();
    }
}