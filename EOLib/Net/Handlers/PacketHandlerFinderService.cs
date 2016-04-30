// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.Handlers
{
	public class PacketHandlerFinderService : IPacketHandlerFinderService
	{
		//todo: take all packet handlers as constructor injected parameter, store in map to their specified family/action

		public bool HandlerExists(PacketFamily family, PacketAction action)
		{
			throw new System.NotImplementedException();
		}

		public IPacketHandler FindHandler(PacketFamily family, PacketAction action)
		{
			throw new System.NotImplementedException();
		}
	}
}