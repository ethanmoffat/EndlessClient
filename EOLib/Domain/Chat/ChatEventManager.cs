// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    //this class sort of behaves like a repository, but for events that occur
    //      in the library that need to update the UI in some way
    public class ChatEventManager
    {
        //todo: probably should handle speech bubbles from other characters/npcs in a similar way

        public event ChatPMTargetNotFoundDelegate ChatPMTargetNotFound = delegate { };

        public event PlayerMutedDelegate PlayerMuted = delegate { };

        internal void FireChatPMTargetNotFound(string pmTargetCharacter)
        {
            ChatPMTargetNotFound(pmTargetCharacter);
        }

        internal void FirePlayerMuted(string adminName)
        {
            PlayerMuted(adminName);
        }
    }

    public delegate void ChatPMTargetNotFoundDelegate(string pmTargetCharacter);

    public delegate void PlayerMutedDelegate(string adminName);
}
