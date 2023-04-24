using System.Collections.Generic;
using System;
using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class SpellFileLoadService : IPubLoadService<ESFRecord>
    {
        private readonly IPubFileDeserializer _pubFileDeserializer;

        public SpellFileLoadService(IPubFileDeserializer pubFileDeserializer)
        {
            _pubFileDeserializer = pubFileDeserializer;
        }

        public IEnumerable<IPubFile<ESFRecord>> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PubDirectory, PubFileNameConstants.ESFFilter);
        }

        public IEnumerable<IPubFile<ESFRecord>> LoadPubFromExplicitFile(string directory, string searchPattern)
        {
            var files = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
                throw new ArgumentException($"No pub files matching {searchPattern} were found in {directory}", nameof(searchPattern));

            Array.Sort(files);

            int fileId = 1;
            foreach (var file in files)
            {
                var fileBytes = File.ReadAllBytes(file);
                yield return _pubFileDeserializer.DeserializeFromByteArray(fileId++, fileBytes, () => new ESFFile());
            }
        }
    }
}
