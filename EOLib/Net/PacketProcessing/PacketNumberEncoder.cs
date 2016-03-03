// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Data;

namespace EOLib.Net.PacketProcessing
{
	internal static class PacketNumberEncoder
	{
		private static readonly INumberEncoderService _service;

		static PacketNumberEncoder()
		{
			_service = new NumberEncoderService();
		}

		internal static byte[] Encode(int number, int size)
		{
			return _service.EncodeNumber(number, size);
		}

		internal static int Decode(params byte[] rawNumber)
		{
			return _service.DecodeNumber(rawNumber);
		}
	}
}
