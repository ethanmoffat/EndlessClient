﻿using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Shared;
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
            var fName = string.Format(Constants.GFXFormat, (int)file);
#if LINUX || OSX
            return new PEFile(fName);
#else
            return new PEFile(fName, shared: true);
#endif
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
