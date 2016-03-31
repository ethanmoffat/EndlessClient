// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Data.Protocol;

namespace EOLib.Net.Connection
{
	public interface IInitDataGeneratorService
	{
		IInitializationData GetInitData(IPacket packet);
	}
}
