// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Text;
using EOLib.Net;

namespace EOLib
{
	public class Packet
	{
		private static readonly int[] Max = { 253, 64009, 16194277 };
		
		private int readPos = 2;
		private int writePos = 2;
		
		private List<byte> data;

		public PacketFamily Family
		{
			get { return (PacketFamily) data[1]; }
		}

		public PacketAction Action
		{
			get { return (PacketAction) data[0]; }
		}

		public int ReadPos
		{
			get { return readPos; }
			set
			{
				if (value < 0 || value > Length)
					throw new IndexOutOfRangeException("Seek out of range of packet");
				readPos = value;
			}
		}

		public int WritePos
		{
			get
			{
				return writePos;
			}
			set
			{
				if (value < 0 || value > Length)
					throw new IndexOutOfRangeException("Seek out of range of packet");
				writePos = value;
			}
		}

		public int Length
		{
			get
			{
				return data.Count;
			}
		}

		public byte[] Data { get { return data.ToArray(); } }

		public Packet(PacketFamily family, PacketAction action)
		{
			data = new List<byte>(2) {(byte) action, (byte) family};
		}

		public Packet(IEnumerable<byte> data)
		{
			this.data = new List<byte>(data);
		}

		public static byte[] EncodeNumber(int number, int size)
		{
			byte[] numArray = new byte[size];
			for (int index = 3; index >= 1; --index)
			{
				if (index >= numArray.Length)
				{
					if (number >= Max[index - 1])
						number %= Max[index - 1];
				}
				else if (number >= Max[index - 1])
				{
					numArray[index] = (byte)(number / Max[index - 1] + 1);
					number %= Max[index - 1];
				}
				else
					numArray[index] = 254;
			}
			numArray[0] = (byte)(number + 1);
			return numArray;
		}

		public static int DecodeNumber(params byte[] b)
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
				num += b[index] * Max[index - 1];
			return num + b[0];
		}

// ReSharper disable once UnusedMember.Global
		public void Clear()
		{
			data = new List<byte>(new[] {data[0], data[1]});
			readPos = 2;
			writePos = 2;
		}

// ReSharper disable once UnusedMember.Global
		public void SetID(PacketFamily family, PacketAction action)
		{
			data[0] = (byte)action;
			data[1] = (byte)family;
		}

		public void AddByte(byte b)
		{
			data.Insert(writePos, b);
			++writePos;
		}

// ReSharper disable once MemberCanBePrivate.Global
		public void AddBreak()
		{
			AddByte(byte.MaxValue);
		}

		public void AddChar(byte c)
		{
			data.InsertRange(writePos, EncodeNumber(c, 1));
			++writePos;
		}

		public void AddShort(short s)
		{
			data.InsertRange(writePos, EncodeNumber(s, 2));
			writePos += 2;
		}

		public void AddThree(int t)
		{
			data.InsertRange(writePos, EncodeNumber(t, 3));
			writePos += 3;
		}

		public void AddInt(int i)
		{
			data.InsertRange(writePos, EncodeNumber(i, 4));
			writePos += 4;
		}

		public void AddBytes(byte[] b)
		{
			data.InsertRange(writePos, b);
			writePos += b.Length;
		}

// ReSharper disable once UnusedMember.Global
		public void AddBytes(byte[] b, int offset, int count)
		{
			byte[] numArray = new byte[count];
			Array.Copy(b, offset, numArray, 0, count);
			data.InsertRange(writePos, numArray);
			writePos += count;
		}

		public void AddString(string s)
		{
			AddBytes(Encoding.ASCII.GetBytes(s));
		}

		public void AddBreakString(string s)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			for (int index = 0; index < bytes.Length; ++index)
			{
				if (bytes[index] == byte.MaxValue)
					bytes[index] = 121;
			}
			AddBytes(bytes);
			AddBreak();
		}

		public byte PeekByte()
		{
			return data[readPos];
		}

		public byte PeekChar()
		{
			return (byte)DecodeNumber(data.GetRange(readPos, 1).ToArray());
		}

		public short PeekShort()
		{
			return (short)DecodeNumber(data.GetRange(readPos, 2).ToArray());
		}

		public int PeekThree()
		{
			return DecodeNumber(data.GetRange(readPos, 3).ToArray());
		}

		public int PeekInt()
		{
			return DecodeNumber(data.GetRange(readPos, 4).ToArray());
		}

// ReSharper disable once MemberCanBePrivate.Global
		public byte[] PeekBytes(int length)
		{
			return data.GetRange(readPos, length).ToArray();
		}

		public string PeekFixedString(int length)
		{
			return Encoding.ASCII.GetString(PeekBytes(length));
		}

		public string PeekEndString()
		{
			return PeekFixedString(Length - readPos);
		}

		public string PeekBreakString()
		{
			int index = readPos;
			while (index < Length && data[index] != byte.MaxValue)
				++index;
			return PeekFixedString(index - readPos);
		}

		public byte GetByte()
		{
			byte num = PeekByte();
			++readPos;
			return num;
		}

		public byte GetChar()
		{
			byte num = PeekChar();
			++readPos;
			return num;
		}

		public short GetShort()
		{
			short num = PeekShort();
			readPos += 2;
			return num;
		}

		public int GetThree()
		{
			int num = PeekThree();
			readPos += 3;
			return num;
		}

		public int GetInt()
		{
			int num = PeekInt();
			readPos += 4;
			return num;
		}

		public byte[] GetBytes(int length)
		{
			byte[] numArray = PeekBytes(length);
			readPos += length;
			return numArray;
		}

		public string GetFixedString(int length)
		{
			return Encoding.ASCII.GetString(GetBytes(length));
		}

		public string GetEndString()
		{
			return GetFixedString(Length - readPos);
		}

		public string GetBreakString()
		{
			string str = PeekBreakString();
			readPos += str.Length + 1;
			return str;
		}

		public void Skip(int bytes)
		{
			readPos += bytes;
		}

		public byte[] Get()
		{
			return data.ToArray();
		}
	}
}
