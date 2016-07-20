// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    public interface IPubFileProvider : IItemFileProvider, INPCFileProvider, ISpellFileProvider, IClassFileProvider
    {
    }

    public interface IItemFileProvider
    {
        IPubFile<EIFRecord> ItemFile { get; }
    }

    public interface INPCFileProvider
    {
        IPubFile<ENFRecord> NPCFile { get; }
    }

    public interface ISpellFileProvider
    {
        IPubFile<ESFRecord> SpellFile { get; }
    }

    public interface IClassFileProvider
    {
        IPubFile<ECFRecord> ClassFile { get; }
    }
}
