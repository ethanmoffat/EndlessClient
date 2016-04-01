// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Config
{
	public interface IConfigurationRepository
	{
		string Host { get; set; }
		int Port { get; set; }

		bool OverrideVersionMajor { get; set; }
		bool OverrideVersionMinor { get; set; }
		bool OverrideVersionBuild { get; set; }
		byte VersionMajor { get; set; }
		byte VersionMinor { get; set; }
		byte VersionBuild { get; set; }
	}

	public interface IConfigurationProvider
	{
		string Host { get; }
		int Port { get; }

		bool OverrideVersionMajor { get; }
		bool OverrideVersionMinor { get; }
		bool OverrideVersionBuild { get; }
		byte VersionMajor { get; }
		byte VersionMinor { get; }
		byte VersionBuild { get; }
	}

	public class ConfigurationRepository : IConfigurationRepository, IConfigurationProvider
	{
		public string Host { get; set; }
		public int Port { get; set; }

		public bool OverrideVersionMajor { get; set; }
		public bool OverrideVersionMinor { get; set; }
		public bool OverrideVersionBuild { get; set; }
		public byte VersionMajor { get; set; }
		public byte VersionMinor { get; set; }
		public byte VersionBuild { get; set; }

		public ConfigurationRepository(IniReader configFile)
		{
			//todo: set all the stuff
		}
	}
}
