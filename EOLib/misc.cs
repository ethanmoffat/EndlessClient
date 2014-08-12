using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//miscellaneous extension functions and some constant values are defined here

namespace EOLib
{
	public static class Hashes
	{
		public static int stupid_hash(int i)
		{
			++i;
			return 110905 + (i % 9 + 1) * ((11092004 - i) % ((i % 11 + 1) * 119)) * 119 + i % 2004;
		}
	}

	public static class ArrayExtension
	{
		public static T[] SubArray<T>(this T[] arr, int offset, int count)
		{
			T[] ret = new T[count];

			if (count == 1)
				ret[0] = arr[offset];

			for (int i = offset; i < offset + count; ++i)
				ret[i - offset] = arr[i];

			return ret;
		}
	}
	
	public static class Constants
	{
		public static readonly int ResponseTimeout = 5000;
		public static readonly int ResponseFileTimeout = 10000;

		public static readonly byte MajorVersion = 0;
		public static readonly byte MinorVersion = 0;
		public static readonly byte ClientVersion = 28;

		public static readonly string Host = "127.0.0.1";
		public static readonly int Port = 8078;

		public static readonly string ItemFilePath = "pub/dat001.eif";
		public static readonly string NPCFilePath = "pub/dtn001.enf";
		public static readonly string SpellFilePath = "pub/dsl001.esf";
		public static readonly string ClassFilePath = "pub/dat001.ecf";
	}

	public static class ConfigStrings
	{
		//TODO: Add the string sections/keys for the configuration file here
	}
}
