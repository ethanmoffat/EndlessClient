// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.Handlers
{
	public interface IPacketHandlerFinderService
	{
		bool HandlerExists(PacketFamily family, PacketAction action);

		IPacketHandler FindHandler(PacketFamily family, PacketAction action);
	}
}
