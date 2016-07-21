// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public class PubFileSaveService : IPubFileSaveService
    {
        private readonly INumberEncoderService _numberEncoderService;

        public PubFileSaveService(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public void SaveFile(string path, IPubFile pubFile)
        {
            var pubFileBytes = pubFile.SerializeToByteArray(_numberEncoderService);

            File.WriteAllBytes(path, pubFileBytes);
        }
    }
}