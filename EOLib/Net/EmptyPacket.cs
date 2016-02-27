// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net
{
	public class EmptyPacket : Packet
	{
		private static readonly IReadOnlyList<byte> _emptyBytes = new List<byte>{ byte.MaxValue, byte.MinValue };

		public EmptyPacket() : base(_emptyBytes.ToList())
		{
		}
	}
}
