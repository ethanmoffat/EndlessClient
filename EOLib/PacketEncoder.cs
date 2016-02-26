// Original Work Copyright (c) Ethan Moffat 2014-2016
// Some of this work is reverse-engineered from 
//	 EOHAX C# DLLs written by Sausage (www.tehsausage.com)
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Net;

namespace EOLib
{
	public sealed class PacketEncoder
	{
		public int SequenceStart { get; set; }
		public int SequenceValue { get; set; }

		public byte RecvMultiple { get; set; }
		public byte SendMultiple { get; set; }

		public PacketEncoder()
		{
			RecvMultiple = SendMultiple = 0;
		}

		public void SetMulti(byte recvMulti, byte sendMulti)
		{
			if (RecvMultiple != 0 || SendMultiple != 0)
				throw new InvalidOperationException("PacketEncoder send/receive multipliers already set");

			RecvMultiple = recvMulti;
			SendMultiple = sendMulti;
		}

		public void Encode(ref byte[] original)
		{
			if (SendMultiple == 0 || (original.Length > 2 && (PacketFamily)original[2] == PacketFamily.Init && (PacketAction)original[3] == PacketAction.Init))
				return;

			AddSequenceByte(ref original);
			SwapMultiples(ref original, SendMultiple);
			Interleave(ref original);
			FlipMSB(ref original);
		}

		public void Decode(ref byte[] original)
		{
			if (RecvMultiple == 0 || ((PacketFamily)original[0] == PacketFamily.Init && (PacketAction)original[1] == PacketAction.Init))
				return;

			FlipMSB(ref original);
			Deinterleave(ref original);
			SwapMultiples(ref original, RecvMultiple);
		}

		private int GetSequence()
		{
			SequenceValue = (SequenceValue + 1) % 10;
			int ret = SequenceStart + SequenceValue;
			return ret;
		}

		private void AddSequenceByte(ref byte[] original)
		{
			int seq = GetSequence();
			int numberOfSequenceBytes = SequenceStart >= 253 ? 2 : 1;

			byte[] newArray = new byte[original.Length + numberOfSequenceBytes];

			Array.Copy(original, 0, newArray, 0, 2); //family/action copied to [0][1]
			Array.Copy(Packet.EncodeNumber(seq, numberOfSequenceBytes), 0, newArray, 2, numberOfSequenceBytes); //sequence number copied to [2](and [3] if it's a larger number)
			if (original.Length > 2) //if there is data left to be copied...
				Array.Copy(original, 2, newArray, 2 + numberOfSequenceBytes, original.Length - 2); //rest of data copied to ([3] or [4] onward)[...]

			original = newArray;
		}

		private static void Interleave(ref byte[] b)
		{
			byte[] numArray = new byte[b.Length];
			int index1 = 0;
			int num = 0;
			while (index1 < b.Length)
			{
				numArray[index1] = b[num++];
				index1 += 2;
			}
			int index2 = index1 - 1;
			if (b.Length % 2 != 0)
				index2 -= 2;
			while (index2 >= 0)
			{
				numArray[index2] = b[num++];
				index2 -= 2;
			}
			numArray.CopyTo(b, 0);
		}

		private static void Deinterleave(ref byte[] b)
		{
			byte[] numArray = new byte[b.Length];
			int index1 = 0;
			int num = 0;
			while (index1 < b.Length)
			{
				numArray[num++] = b[index1];
				index1 += 2;
			}
			int index2 = index1 - 1;
			if (b.Length % 2 != 0)
				index2 -= 2;
			while (index2 >= 0)
			{
				numArray[num++] = b[index2];
				index2 -= 2;
			}
			numArray.CopyTo(b, 0);
		}

		private static void FlipMSB(ref byte[] b)
		{
			for (int index = 0; index < b.Length; ++index)
				b[index] = (byte)(b[index] ^ 128U);
		}

		private static void SwapMultiples(ref byte[] b, int multi)
		{
			int num1 = 0;
			if (multi <= 0)
				return;
			for (int index1 = 0; index1 <= b.Length; ++index1)
			{
				if (index1 != b.Length && b[index1] % multi == 0)
				{
					++num1;
				}
				else
				{
					if (num1 > 1)
					{
						for (int index2 = 0; index2 < num1 / 2; ++index2)
						{
							byte num2 = b[index1 - num1 + index2];
							b[index1 - num1 + index2] = b[index1 - index2 - 1];
							b[index1 - index2 - 1] = num2;
						}
					}
					num1 = 0;
				}
			}
		}
	}
}
