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
        Server, //server is saying something
        Command, //local command: #
    }
}