// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.PacketProcessing
{
	public class PacketEncoderRepository : IPacketEncoderRepository
	{
		public byte ReceiveMultiplier { get; set; }
		public byte SendMultiplier { get; set; }
	}
}
