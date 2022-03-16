using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public interface IPubLoadService<T>
        where T : class, IPubRecord, new()
    {
        IPubFile<T> LoadPubFromDefaultFile();

        IPubFile<T> LoadPubFromExplicitFile(string fileName);
    }
}
