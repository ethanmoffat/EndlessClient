// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    //todo: rename these to IEIFFileRepository, etc. (same for providers and impl)
    public interface IPubFileRepository : IItemFileRepository, INPCFileRepository, ISpellFileRepository, IClassFileRepository
    {
    }

    public interface IItemFileRepository
    {
        IPubFile<EIFRecord> ItemFile { get; set; }
    }

    public interface INPCFileRepository
    {
        IPubFile<ENFRecord> NPCFile { get; set; }
    }

    public interface ISpellFileRepository
    {
        IPubFile<ESFRecord> SpellFile { get; set; }
    }

    public interface IClassFileRepository
    {
        IPubFile<ECFRecord> ClassFile { get; set; }
    }
}
