// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;

namespace EOLib.Net.Builders
{
    public interface IChatPacketBuilder
    {
        IPacket BuildChatPacket(ChatType chatType,
                                  string chat,
                                  string targetCharacter);
    }
}
