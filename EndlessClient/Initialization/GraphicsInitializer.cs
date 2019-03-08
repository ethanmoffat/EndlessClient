using AutomaticTypeMapper;
using EOLib.Graphics;
using PELoaderLib;
using System.IO;

namespace EndlessClient.Initialization
{
    [MappedType(BaseType = typeof(IGameInitializer))]
    public class GraphicsInitializer : IGameInitializer
    {
        private readonly IPEFileCollection _peFileCollection;

        public GraphicsInitializer(IPEFileCollection peFileCollection)
        {
            _peFileCollection = peFileCollection;
        }

        public void Initialize()
        {
            _peFileCollection.PopulateCollectionWithStandardGFX();

            foreach (var filePair in _peFileCollection)
                TryInitializePEFiles(filePair.Key, filePair.Value);
        }

        private static void TryInitializePEFiles(GFXTypes file, IPEFile peFile)
        {
            var number = ((int)file).ToString("D3");

            try
            {
                peFile.Initialize();
            }
            catch (IOException)
            {
                throw new LibraryLoadException(number, file);
            }

            if (!peFile.Initialized)
                throw new LibraryLoadException(number, file);
        }
    }
}
