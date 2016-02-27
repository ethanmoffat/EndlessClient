// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net
{
	public interface IPacketEncoderService
	{
		byte[] PrependLengthBytes(byte[] data);

		Packet AddSequenceNumber(Packet pkt, int sequenceNumber);

		byte[] Encode(Packet original, byte encodeMultiplier);

		Packet Decode(byte[] original, byte decodeMultiplier);
	}
}
