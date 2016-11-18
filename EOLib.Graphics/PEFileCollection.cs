// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PELoaderLib;

namespace EOLib.Graphics
{
    public sealed class PEFileCollection : Dictionary<GFXTypes, IPEFile>, IPEFileCollection
    {
        public PEFileCollection()
        {
            var gfxTypes = Enum.GetValues(typeof(GFXTypes)).OfType<GFXTypes>();
            var modules = gfxTypes.ToDictionary(type => type, CreateGFXFile);
            foreach(var modulePair in modules)
                Add(modulePair.Key, modulePair.Value);
        }

        private IPEFile CreateGFXFile(GFXTypes file)
        {
            var number = ((int)file).ToString("D3");
            var fName = Path.Combine("gfx", "gfx" + number + ".egf");

            return new PEFile(fName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PEFileCollection()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                foreach (var pair in this)
                    pair.Value.Dispose();
        }
    }

    public interface IPEFileCollection : IDictionary<GFXTypes, IPEFile>, IDisposable
    {
    }
}
