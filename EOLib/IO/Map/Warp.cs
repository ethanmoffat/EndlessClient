// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Data.Map;

namespace EOLib.IO.Map
{
	public class Warp : IMapElement
	{
		public byte x, y;
		public short warpMap;
		public byte warpX;
		public byte warpY;
		public byte levelRequirement;
		public DoorSpec door;
		public bool doorOpened;
		public bool backOff; //used in code only: determines whether a door packet was recently sent for this particular door (only valid for doors)
	}
}