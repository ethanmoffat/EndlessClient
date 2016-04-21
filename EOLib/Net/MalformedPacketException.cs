// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net
{
	public class MalformedPacketException : Exception
	{
		public IPacket Packet { get; private set; }

		public MalformedPacketException(string message, IPacket packet)
			: base(message)
		{
			Packet = packet;
		}
	}
}
