// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Config
{
	public interface IClientVersionProvider
	{
		byte VersionMajor { get; }
		byte VersionMinor { get; }
		byte VersionBuild { get; }
	}
}
