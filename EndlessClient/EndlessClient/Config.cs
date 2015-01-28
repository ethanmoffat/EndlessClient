using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using System.IO;

namespace EndlessClient
{	
	public static class Config
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, UInt32 VolumeNameSize, ref UInt32 VolumeSerialNumber, ref UInt32 MaximumComponentLength, ref UInt32 FileSystemFlags, StringBuilder FileSystemNameBuffer, UInt32 FileSystemNameSize);

		public static string GetHDDSerial()
		{
			string strDriveLetter = System.Windows.Forms.Application.UserAppDataPath[0] + ":\\";
			StringBuilder VolLabel = new StringBuilder(256); // Label
			uint serNum = 0;
			uint maxCompLen = 0;
			uint VolFlags = 0;
			StringBuilder FSName = new StringBuilder(256); // File System Name

			if(GetVolumeInformation(strDriveLetter, VolLabel, (UInt32)VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity) != 0)
				return Convert.ToString(serNum);
			
			return "";
		}

	}

	public class IniReader
	{
		private readonly SortedList<string, SortedList<string, object>> sections = new SortedList<string, SortedList<string, object>>();
		public string Filename { get; private set; }

		public IniReader(string filename) { Filename = filename; }

		public bool Load()
		{
			try
			{
				using (StreamReader str = new StreamReader(Filename))
				{
					string header = "";
					while (!str.EndOfStream)
					{
						string nextLine = str.ReadLine();
						if (string.IsNullOrEmpty(nextLine) || nextLine[0] == '\'' || nextLine[0] == '#')
						{
							continue;
						}
						
						if (nextLine.Contains('#')) //remove any comment from the header
						{
							nextLine = nextLine.Remove(nextLine.IndexOf('#'));
						}
						else if (nextLine.Contains('\''))
						{
							nextLine = nextLine.Remove(nextLine.IndexOf('\''));
						}
						nextLine = nextLine.Trim();

						if (nextLine[0] == '[' && nextLine[nextLine.Length - 1] == ']')
						{
							header = nextLine.Remove(0, 1);
							header = header.Remove(header.Length - 1);
							sections.Add(header, new SortedList<string, object>());
						}
						else
						{
							//        0    5    10   15  19
							//format: identifier(datatype)=value #or identifier ( datatype   ) =    value
							//get type string
							int dTypeStart = nextLine.IndexOf('(') + 1;
							int dTypeLen = nextLine.IndexOf(')') - dTypeStart;
							string typeString;
							if (dTypeStart < 0 || dTypeLen < 0)
							{
								typeString = "string";
							}
							else if (dTypeStart < 1 || dTypeLen < 2 || dTypeStart >= nextLine.Length || dTypeLen >= nextLine.Length)
							{
								continue;
							}
							else
							{
								typeString = nextLine.Substring(dTypeStart, dTypeLen);
								nextLine = nextLine.Remove(dTypeStart - 1, dTypeLen + 2);
							}
							//get the pair of identifier/value
							string[] pair = nextLine.Split(new [] { '=' });
							if (pair.Length != 2)
								continue;
							//add to the database
							switch (typeString.ToLower())
							{
								case "int":
									int i_res;
									if (!int.TryParse(pair[1], out i_res))
										continue;
									sections[header].Add(pair[0], i_res);
									break;
								case "bool":
									bool b_res;
									if (!bool.TryParse(pair[1], out b_res))
										continue;
									sections[header].Add(pair[0], b_res);
									break;
								default://default to string
									sections[header].Add(pair[0], pair[1]);
									break;
							}
						}
					}
				}
			}
			catch
			{
				if (!Directory.Exists(Directory.GetParent(Filename).FullName))
				{
					Directory.CreateDirectory(Directory.GetParent(Filename).FullName);
				}
				if (!File.Exists(Filename))
				{
					File.Create(Filename).Close();
				}
				return false;
			}

			return true;
		}

		public bool GetValue(string section, string identifier, out object value)
		{
			value = null;
			if (!sections.ContainsKey(section))
				return false;
			if (!sections[section].ContainsKey(identifier))
				return false;

			value = sections[section][identifier];
			return true;
		}
		public bool GetValue(string identifier, out object value)
		{
			return GetValue("", identifier, out value);
		}

		public bool GetValue(string section, string identifier, out string value)
		{
			value = null;
			if (!sections.ContainsKey(section))
				return false;
			if (!sections[section].ContainsKey(identifier))
				return false;

			value = sections[section][identifier] as string;

			return true;
		}
		public bool GetValue(string identifier, out string value)
		{
			return GetValue("", identifier, out value);
		}

		public bool GetValue(string section, string identifier, out int value)
		{
			value = int.MaxValue;
			if (!sections.ContainsKey(section))
				return false;
			if (!sections[section].ContainsKey(identifier))
				return false;

			value = Convert.ToInt32(sections[section][identifier]);

			return true;
		}
		public bool GetValue(string identifier, out int value)
		{
			return GetValue("general", identifier, out value);
		}

		public bool GetValue(string section, string identifier, out bool value)
		{
			value = false;
			if (!sections.ContainsKey(section))
				return false;
			if (!sections[section].ContainsKey(identifier))
				return false;

			value = Convert.ToBoolean((sections[section][identifier]));

			return true;
		}
		public bool GetValue(string identifier, out bool value)
		{
			return GetValue("general", identifier, out value);
		}
	}
}
