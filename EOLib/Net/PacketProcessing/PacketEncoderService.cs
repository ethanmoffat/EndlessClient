// Original Work Copyright (c) Ethan Moffat 2014-2016
// Some of this work is reverse-engineered from 
//	 EOHAX C# DLLs written by Sausage (www.tehsausage.com)
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net.PacketProcessing
{
	public sealed class PacketEncoderService : IPacketEncoderService
	{
		public byte[] PrependLengthBytes(byte[] data)
		{
			var ret = PrependLength(data.ToList());
			return ret.ToArray();
		}

		public OldPacket AddSequenceNumber(OldPacket pkt, int sequenceNumber)
		{
			var byteList = pkt.Data.ToList();
			byteList = AddSequenceBytes(byteList, sequenceNumber);
			return new OldPacket(byteList);
		}

		public byte[] Encode(OldPacket original, byte encodeMultiplier)
		{
			if (encodeMultiplier == 0 || PacketInvalidForEncode(original))
				return original.Data.ToArray();

			var byteList = original.Data.ToList();
			byteList = SwapMultiples(byteList, encodeMultiplier);
			byteList = Interleave(byteList);
			byteList = FlipMSB(byteList);

			return byteList.ToArray();
		}

		public OldPacket Decode(byte[] original, byte decodeMultiplier)
		{
			if (decodeMultiplier == 0 || PacketInvalidForDecode(original))
				return new OldPacket(original);

			var byteList = original.ToList();
			byteList = FlipMSB(byteList);
			byteList = Deinterleave(byteList);
			byteList = SwapMultiples(byteList, decodeMultiplier);

			return new OldPacket(byteList);
		}

		public byte[] EncodeNumber(int number, int size)
		{
			byte[] numArray = new byte[size];
			for (int index = 3; index >= 1; --index)
			{
				if (index >= numArray.Length)
				{
					if (number >= Constants.NUMERIC_MAXIMUM[index - 1])
						number %= Constants.NUMERIC_MAXIMUM[index - 1];
				}
				else if (number >= Constants.NUMERIC_MAXIMUM[index - 1])
				{
					numArray[index] = (byte) (number/Constants.NUMERIC_MAXIMUM[index - 1] + 1);
					number %= Constants.NUMERIC_MAXIMUM[index - 1];
				}
				else
					numArray[index] = 254;
			}
			numArray[0] = (byte) (number + 1);
			return numArray;
		}

		public int DecodeNumber(params byte[] b)
		{
			for (int index = 0; index < b.Length; ++index)
			{
				if (b[index] == 254)
					b[index] = 1;
				else if (b[index] == 0)
					b[index] = 128;
				--b[index];
			}

			int num = 0;
			for (int index = b.Length - 1; index >= 1; --index)
				num += b[index]*Constants.NUMERIC_MAXIMUM[index - 1];
			return num + b[0];
		}

		#region Packet Validation

		private bool PacketInvalidForEncode(OldPacket pkt)
		{
			return IsInitPacket(pkt);
		}

		private bool PacketInvalidForDecode(byte[] data)
		{
			return data.Length > 2 && IsInitPacket(new OldPacket(new[] {data[3], data[2]}));
		}

		private static bool IsInitPacket(OldPacket pkt)
		{
			return pkt.Family == PacketFamily.Init &&
			       pkt.Action == PacketAction.Init;
		}

		#endregion

		#region Sequence Byte(s)

		private List<byte> AddSequenceBytes(IReadOnlyList<byte> original, int seq)
		{
			var numberOfSequenceBytes = seq >= Constants.ONE_BYTE_MAX ? 2 : 1;
			var encodedSequenceBytes = EncodeNumber(seq, numberOfSequenceBytes);

			var combined = new List<byte>(original.Count + numberOfSequenceBytes);
			//family/action copied to [0][1]
			combined.AddRange(new[] {original[0], original[1]});
			//sequence number copied to [2] (and [3] if it's a two-byte number)
			combined.AddRange(encodedSequenceBytes);
			//add the remaining data - rest of data copied to [3] (or [4]) onward [...]
			combined.AddRange(original.Where((b, i) => i >= 2));

			return combined;
		}

		#endregion

		#region Length Bytes

		private List<byte> PrependLength(IReadOnlyList<byte> data)
		{
			var len = EncodeNumber(data.Count, 2);
			var combined = new List<byte>(data.Count + len.Length);

			combined.AddRange(len);
			combined.AddRange(data);

			return combined;
		}

		#endregion

		#region Encode/Decode

		private static List<byte> Interleave(IReadOnlyList<byte> data)
		{
			var numArray = new byte[data.Count];
			var index1 = 0;
			var num = 0;

			while (index1 < data.Count)
			{
				numArray[index1] = data[num++];
				index1 += 2;
			}

			var index2 = index1 - 1;
			if (data.Count % 2 != 0)
				index2 -= 2;

			while (index2 >= 0)
			{
				numArray[index2] = data[num++];
				index2 -= 2;
			}

			return numArray.ToList();
		}

		private static List<byte> Deinterleave(IReadOnlyList<byte> data)
		{
			var numArray = new byte[data.Count];
			var index1 = 0;
			var num = 0;

			while (index1 < data.Count)
			{
				numArray[num++] = data[index1];
				index1 += 2;
			}

			var index2 = index1 - 1;
			if (data.Count % 2 != 0)
				index2 -= 2;

			while (index2 >= 0)
			{
				numArray[num++] = data[index2];
				index2 -= 2;
			}

			return numArray.ToList();
		}

		private static List<byte> FlipMSB(IReadOnlyList<byte> data)
		{
			return data.Select(x => (byte)(x ^ 0x80u)).ToList();
		}

		private static List<byte> SwapMultiples(IReadOnlyList<byte> data, int multi)
		{
			int num1 = 0;

			var result = data.ToArray();

			for (int index1 = 0; index1 <= data.Count; ++index1)
			{
				if (index1 != data.Count && data[index1] % multi == 0)
				{
					++num1;
				}
				else
				{
					if (num1 > 1)
					{
						for (int index2 = 0; index2 < num1 / 2; ++index2)
						{
							byte num2 = data[index1 - num1 + index2];
							result[index1 - num1 + index2] = data[index1 - index2 - 1];
							result[index1 - index2 - 1] = num2;
						}
					}
					num1 = 0;
				}
			}

			return result.ToList();
		}

		#endregion
	}
}
