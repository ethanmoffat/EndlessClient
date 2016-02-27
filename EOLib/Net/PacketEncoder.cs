// Original Work Copyright (c) Ethan Moffat 2014-2016
// Some of this work is reverse-engineered from 
//	 EOHAX C# DLLs written by Sausage (www.tehsausage.com)
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net
{
	public sealed class PacketEncoder
	{
		public int SequenceStart { get; set; }
		public int SequenceValue { get; set; }

		public byte RecvMultiple { get; set; }
		public byte SendMultiple { get; set; }

		public void SetMulti(byte recvMulti, byte sendMulti)
		{
			if (RecvMultiple != 0 || SendMultiple != 0)
				throw new InvalidOperationException("PacketEncoder send/receive multipliers already set");

			RecvMultiple = recvMulti;
			SendMultiple = sendMulti;
		}

		public byte[] Encode(Packet original)
		{
			if (PacketInvalidForEncode(original))
				return PrependLength(original.Data).ToArray();

			var byteList = original.Data.ToList();
			byteList = AddSequenceByte(byteList);
			byteList = SwapMultiples(byteList, SendMultiple);
			byteList = Interleave(byteList);
			byteList = FlipMSB(byteList);
			byteList = PrependLength(byteList);

			return byteList.ToArray();
		}

		public Packet Decode(byte[] original)
		{
			if (PacketInvalidForDecode(original))
				return new Packet(original);

			var byteList = original.ToList();
			byteList = FlipMSB(byteList);
			byteList = Deinterleave(byteList);
			byteList = SwapMultiples(byteList, RecvMultiple);

			return new Packet(byteList);
		}

		#region Packet Validation

		private bool PacketInvalidForEncode(Packet pkt)
		{
			return SendMultiple == 0 || IsInitPacket(pkt);
		}

		private bool PacketInvalidForDecode(byte[] data)
		{
			return RecvMultiple == 0 || data.Length > 2 && IsInitPacket(new Packet(new[] {data[3], data[2]}));
		}

		private static bool IsInitPacket(Packet pkt)
		{
			return pkt.Family == PacketFamily.Init &&
			       pkt.Action == PacketAction.Init;
		}

		#endregion

		#region Sequence Byte(s)

		private List<byte> AddSequenceByte(IReadOnlyList<byte> original)
		{
			var seq = GetNextSeqIncrement();
			var numberOfSequenceBytes = SequenceStart >= 253 ? 2 : 1;
			var encodedSequenceBytes = Packet.EncodeNumber(seq, numberOfSequenceBytes);

			var combined = new List<byte>(original.Count + numberOfSequenceBytes);
			//family/action copied to [0][1]
			combined.AddRange(new[] {original[0], original[1]});
			//sequence number copied to [2] (and [3] if it's a two-byte number)
			combined.AddRange(encodedSequenceBytes);
			//add the remaining data - rest of data copied to [3] (or [4]) onward [...]
			combined.AddRange(original.Where((b, i) => i >= 2));

			return combined;
		}

		private int GetNextSeqIncrement()
		{
			SequenceValue = (SequenceValue + 1) % 10;
			int ret = SequenceStart + SequenceValue;
			return ret;
		}

		#endregion

		#region Length Bytes

		private List<byte> PrependLength(IReadOnlyList<byte> data)
		{
			var len = Packet.EncodeNumber(data.Count, 2);
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
