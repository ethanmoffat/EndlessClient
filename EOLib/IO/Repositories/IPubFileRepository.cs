// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Repositories
{
	public interface IPubFileRepository : IItemFileRepository, INPCFileRepository, ISpellFileRepository, IClassFileRepository
	{
	}

	public interface IItemFileRepository
	{
		IDataFile<ItemRecord> ItemFile { get; set; }
	}

	public interface INPCFileRepository
	{
		IDataFile<NPCRecord> NPCFile { get; set; }
	}

	public interface ISpellFileRepository
	{
		IDataFile<SpellRecord> SpellFile { get; set; }
	}

	public interface IClassFileRepository
	{
		IDataFile<ClassRecord> ClassFile { get; set; }
	}
}
