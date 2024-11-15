using System;
using System.Collections.Generic;
using System.IO;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Shared;

namespace EOLib.IO.Services
{
    public abstract class BasePubLoadService<TRecord> : IPubLoadService<TRecord>
        where TRecord : class, IPubRecord, new()
    {
        private readonly IPubFileDeserializer _pubFileDeserializer;

        protected abstract string FileFilter { get; }

        protected BasePubLoadService(IPubFileDeserializer pubFileDeserializer)
        {
            _pubFileDeserializer = pubFileDeserializer;
        }

        public IEnumerable<IPubFile<TRecord>> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(Constants.PubDirectory, FileFilter);
        }

        public IEnumerable<IPubFile<TRecord>> LoadPubFromExplicitFile(string directory, string searchPattern)
        {
            var files = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
                throw new ArgumentException($"No pub files matching {searchPattern} were found in {directory}", nameof(searchPattern));

            Array.Sort(files);

            int fileId = 1;
            foreach (var file in files)
            {
                var fileBytes = File.ReadAllBytes(file);
                yield return _pubFileDeserializer.DeserializeFromByteArray(fileId++, fileBytes, Factory);
            }
        }

        protected abstract IPubFile<TRecord> Factory();
    }
}
