// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Repositories
{
    public interface IPubFileProvider : IItemFileProvider, INPCFileProvider, ISpellFileProvider, IClassFileProvider
    {
    }

    public interface IItemFileProvider
    {
        IDataFile<ItemRecord> ItemFile { get; }
    }

    public interface INPCFileProvider
    {
        IDataFile<NPCRecord> NPCFile { get; }
    }

    public interface ISpellFileProvider
    {
        IDataFile<SpellRecord> SpellFile { get; }
    }

    public interface IClassFileProvider
    {
        IDataFile<ClassRecord> ClassFile { get; }
    }
}
