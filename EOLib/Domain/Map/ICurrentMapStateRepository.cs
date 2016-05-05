// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Map
{
	public interface ICurrentMapStateRepository
	{
		short CurrentMapID { get; set; }
	}

	public interface ICurrentMapProvider
	{
		short CurrentMapID { get; }
	}

	public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapProvider
	{
		public short CurrentMapID { get; set; }
	}
}
