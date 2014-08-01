using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOLib
{
	public class Packet
	{
		public static readonly int[] Max = new int[3] { 253, 64009, 16194277 };
		private int readPos = 2;
		private int writePos = 2;
		public const byte Break = (byte)255;
		private List<byte> data;

		public PacketFamily Family
		{
			get
			{
				return (PacketFamily)this.data[1];
			}
			set
			{
				this.data[1] = (byte)value;
			}
		}

		public PacketAction Action
		{
			get
			{
				return (PacketAction)this.data[0];
			}
			set
			{
				this.data[0] = (byte)value;
			}
		}

		public int ReadPos
		{
			get
			{
				return this.readPos;
			}
			set
			{
				if (value < 0 || value > this.Length)
					throw new IndexOutOfRangeException("Seek out of range of packet");
				this.readPos = value;
			}
		}

		public int WritePos
		{
			get
			{
				return this.writePos;
			}
			set
			{
				if (value < 0 || value > this.Length)
					throw new IndexOutOfRangeException("Seek out of range of packet");
				this.writePos = value;
			}
		}

		public int Length
		{
			get
			{
				return this.data.Count;
			}
		}

		public Packet(PacketFamily family, PacketAction action)
		{
			this.data = new List<byte>(2);
			this.data.Add((byte)action);
			this.data.Add((byte)family);
		}

		public Packet(byte[] data)
		{
			this.data = new List<byte>((IEnumerable<byte>)data);
		}

		public static byte[] EncodeNumber(int number, int size)
		{
			byte[] numArray = new byte[size];
			for (int index = 3; index >= 1; --index)
			{
				if (index >= numArray.Length)
				{
					if (number >= Packet.Max[index - 1])
						number %= Packet.Max[index - 1];
				}
				else if (number >= Packet.Max[index - 1])
				{
					numArray[index] = (byte)(number / Packet.Max[index - 1] + 1);
					number %= Packet.Max[index - 1];
				}
				else
					numArray[index] = (byte)254;
			}
			numArray[0] = (byte)(number + 1);
			return numArray;
		}

		public static int DecodeNumber(byte[] b)
		{
			for (int index = 0; index < b.Length; ++index)
			{
				if ((int)b[index] == 0 || (int)b[index] == 254)
					b[index] = (byte)0;
				else
					--b[index];
			}
			int num = 0;
			for (int index = b.Length - 1; index >= 1; --index)
				num += (int)b[index] * Packet.Max[index - 1];
			return num + (int)b[0];
		}

		public void Clear()
		{
			this.data = new List<byte>((IEnumerable<byte>)new byte[2] { this.data[0], this.data[1] });
			this.readPos = 2;
			this.writePos = 2;
		}

		public void SetID(PacketFamily family, PacketAction action)
		{
			this.data[0] = (byte)action;
			this.data[1] = (byte)family;
		}

		public void AddByte(byte b)
		{
			this.data.Insert(this.writePos, b);
			++this.writePos;
		}

		public void AddBreak()
		{
			this.AddByte(byte.MaxValue);
		}

		public void AddChar(byte c)
		{
			this.data.InsertRange(this.writePos, (IEnumerable<byte>)Packet.EncodeNumber((int)c, 1));
			++this.writePos;
		}

		public void AddShort(short s)
		{
			this.data.InsertRange(this.writePos, (IEnumerable<byte>)Packet.EncodeNumber((int)s, 2));
			this.writePos += 2;
		}

		public void AddThree(int t)
		{
			this.data.InsertRange(this.writePos, (IEnumerable<byte>)Packet.EncodeNumber(t, 3));
			this.writePos += 3;
		}

		public void AddInt(int i)
		{
			this.data.InsertRange(this.writePos, (IEnumerable<byte>)Packet.EncodeNumber(i, 4));
			this.writePos += 4;
		}

		public void AddBytes(byte[] b)
		{
			this.data.InsertRange(this.writePos, (IEnumerable<byte>)b);
			this.writePos += b.Length;
		}

		public void AddBytes(byte[] b, int offset, int count)
		{
			byte[] numArray = new byte[count];
			Array.Copy((Array)b, offset, (Array)numArray, 0, count);
			this.data.InsertRange(this.writePos, (IEnumerable<byte>)numArray);
			this.writePos += count;
		}

		public void AddString(string s)
		{
			this.AddBytes(Encoding.ASCII.GetBytes(s));
		}

		public void AddBreakString(string s)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			for (int index = 0; index < bytes.Length; ++index)
			{
				if ((int)bytes[index] == (int)byte.MaxValue)
					bytes[index] = (byte)121;
			}
			this.AddBytes(bytes);
			this.AddBreak();
		}

		public byte PeekByte()
		{
			return this.data[this.readPos];
		}

		public byte PeekChar()
		{
			return (byte)Packet.DecodeNumber(this.data.GetRange(this.readPos, 1).ToArray());
		}

		public short PeekShort()
		{
			return (short)Packet.DecodeNumber(this.data.GetRange(this.readPos, 2).ToArray());
		}

		public int PeekThree()
		{
			return Packet.DecodeNumber(this.data.GetRange(this.readPos, 3).ToArray());
		}

		public int PeekInt()
		{
			return Packet.DecodeNumber(this.data.GetRange(this.readPos, 4).ToArray());
		}

		public byte[] PeekBytes(int length)
		{
			return this.data.GetRange(this.readPos, length).ToArray();
		}

		public string PeekFixedString(int length)
		{
			return Encoding.ASCII.GetString(this.PeekBytes(length));
		}

		public string PeekEndString()
		{
			return this.PeekFixedString(this.Length - this.readPos);
		}

		public string PeekBreakString()
		{
			int index = this.readPos;
			while (index < this.Length && (int)this.data[index] != (int)byte.MaxValue)
				++index;
			return this.PeekFixedString(index - this.readPos);
		}

		public byte GetByte()
		{
			byte num = this.PeekByte();
			++this.readPos;
			return num;
		}

		public byte GetChar()
		{
			byte num = this.PeekChar();
			++this.readPos;
			return num;
		}

		public short GetShort()
		{
			short num = this.PeekShort();
			this.readPos += 2;
			return num;
		}

		public int GetThree()
		{
			int num = this.PeekThree();
			this.readPos += 3;
			return num;
		}

		public int GetInt()
		{
			int num = this.PeekInt();
			this.readPos += 4;
			return num;
		}

		public byte[] GetBytes(int length)
		{
			byte[] numArray = this.PeekBytes(length);
			this.readPos += length;
			return numArray;
		}

		public string GetFixedString(int length)
		{
			return Encoding.ASCII.GetString(this.GetBytes(length));
		}

		public string GetEndString()
		{
			return this.GetFixedString(this.Length - this.readPos);
		}

		public string GetBreakString()
		{
			string str = this.PeekBreakString();
			this.readPos += str.Length + 1;
			return str;
		}

		public void Skip(int bytes)
		{
			this.readPos += bytes;
		}

		public byte[] Get()
		{
			return this.data.ToArray();
		}
	}
}
