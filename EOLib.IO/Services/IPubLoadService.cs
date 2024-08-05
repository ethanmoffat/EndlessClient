using System.Collections.Generic;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public interface IPubLoadService<T>
        where T : class, IPubRecord, new()
    {
        IEnumerable<IPubFile<T>> LoadPubFromDefaultFile();

        IEnumerable<IPubFile<T>> LoadPubFromExplicitFile(string directory, string fileName);
    }
}