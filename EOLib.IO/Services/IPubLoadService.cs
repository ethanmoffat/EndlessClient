using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public interface IPubLoadService<out T>
        where T : class, IPubRecord, new()
    {
        IPubFile<T> LoadPubFromDefaultFile();

        IPubFile<T> LoadPubFromExplicitFile(string fileName);
    }
}
