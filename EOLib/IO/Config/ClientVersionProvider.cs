// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Config
{
	public class ClientVersionProvider : IClientVersionProvider
	{
		public byte VersionMajor { get; private set; }
		public byte VersionMinor { get; private set; }
		public byte VersionBuild { get; private set; }

		public ClientVersionProvider(IConfigurationProvider configProvider)
		{
			VersionMajor = configProvider.OverrideVersionMajor ? configProvider.VersionMajor : Constants.MajorVersion;
			VersionMinor = configProvider.OverrideVersionMinor ? configProvider.VersionMinor : Constants.MinorVersion;
			VersionBuild = configProvider.OverrideVersionBuild ? configProvider.VersionBuild : Constants.ClientVersion;
		}
	}
}
