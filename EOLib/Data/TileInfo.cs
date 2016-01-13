// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Runtime.InteropServices;
using EOLib.IO;

namespace EOLib.Data
{
	[StructLayout(LayoutKind.Explicit)]
	public struct TileInfo
	{
		[FieldOffset(0)]
		public TileInfoReturnType ReturnType;

		//only one of these is valid at a time
		[FieldOffset(4)]
		public TileSpec Spec;

		[FieldOffset(8)]
		public Warp Warp;
		[FieldOffset(8)]
		public NPCRecord NPC;
		[FieldOffset(8)]
		public MapSign Sign;
	}
}
