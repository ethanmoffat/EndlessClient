// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net
{
	public interface IPacketProcessorActions
	{
		void SetInitialSequenceNumber(int seq1, int seq2);

		void SetUpdatedBaseSequenceNumber(int seq1, int seq2);

		void SetEncodeMultiples(byte emulti_d, byte emulti_e);

		byte[] EncodePacket(Packet pkt);

		Packet DecodeData(byte[] rawData);
	}
}
