// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net
{
	public interface IPacketSequenceService
	{
		int CalculateInitialSequenceNumber(int seq1, int seq2);

		int CalculateNextSequenceNumber(int sequence, int increment);
		
		int CalculateNewInitialSequenceNumber(int seq1, int seq2);

		int CalculateNextSequenceIncrement(int increment);
	}
}
