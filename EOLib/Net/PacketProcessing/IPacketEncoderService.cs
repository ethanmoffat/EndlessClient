// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Net.PacketProcessing
{
	public interface IPacketEncoderService
	{
		byte[] PrependLengthBytes(byte[] data);

		OldPacket AddSequenceNumber(OldPacket pkt, int sequenceNumber);

		IPacket AddSequenceNumber(IPacket pkt, int sequenceNumber);

		byte[] Encode(OldPacket original, byte encodeMultiplier);

		byte[] Encode(IPacket original, byte encodeMultiplier);
		
		OldPacket Decode(byte[] original, byte decodeMultiplier);

		IPacket Decode(IEnumerable<byte> original, byte decodeMultiplier);

		byte[] EncodeNumber(int number, int size);

		int DecodeNumber(params byte[] b);
	}
}
