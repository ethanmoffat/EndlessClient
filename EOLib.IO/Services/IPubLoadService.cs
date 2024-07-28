using EOLib.IO.Pub;
using System.Collections.Generic;

namespace EOLib.IO.Services
{
    public interface IPubLoadService<T>
        where T : class, IPubRecord, new()
    {
        IEnumerable<IPubFile<T>> LoadPubFromDefaultFile();

        IEnumerable<IPubFile<T>> LoadPubFromExplicitFile(string directory, string fileName);
    }
}