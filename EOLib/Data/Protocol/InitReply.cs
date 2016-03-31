// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.Protocol
{
	public enum InitReply
	{
		ClientOutOfDate = 1,
		Success = 2,
		BannedFromServer = 3,
		MapFile = 4,
		ItemFile = 5,
		NpcFile = 6,
		SpellFile = 7,
		AllPlayersList = 8,
		MapMutation = 9,
		FriendPlayersList = 10,
		ClassFile = 11,
		ErrorState = 0
	}
}
