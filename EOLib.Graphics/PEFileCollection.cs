// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using AutomaticTypeMapper;
using PELoaderLib;

namespace EOLib.Graphics
{
    [MappedType(BaseType = typeof(IPEFileCollection), IsSingleton = true)]
    public sealed class PEFileCollection : Dictionary<GFXTypes, IPEFile>, IPEFileCollection
    {
        public void PopulateCollectionWithStandardGFX()
        {
            var gfxTypes = (GFXTypes[])Enum.GetValues(typeof(GFXTypes));
            foreach (var type in gfxTypes)
                Add(type, CreateGFXFile(type));
        }

        private IPEFile CreateGFXFile(GFXTypes file)
        {
            var number = ((int)file).ToString("D3");
            var fName = Path.Combine("gfx", "gfx" + number + ".egf");

            return new PEFile(fName);
        }

        public void Dispose()
        {
            foreach (var pair in this)
                pair.Value.Dispose();
        }
    }

    public interface IPEFileCollection : IDictionary<GFXTypes, IPEFile>, IDisposable
    {
        void PopulateCollectionWithStandardGFX();
    }
}
