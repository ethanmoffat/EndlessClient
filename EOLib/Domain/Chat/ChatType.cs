// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Chat
{
    public enum ChatType
    {
        Admin,
        PM, //private message: !
        Local, //local chat: no prefix
        Global,//global chat: ~
        Guild, //guild chat: &
        Party, //party chat: '
        Announce, //global chat: @
        NPC, //npc is saying something
        Server //server is saying something
    }
}