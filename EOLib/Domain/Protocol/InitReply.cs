namespace EOLib.Domain.Protocol
{
    public enum InitReply
    {
        ClientOutOfDate = 1,
        Success = 2,
        BannedFromServer = 3,
        WarpMap = 4,
        MapFile = 5,
        ItemFile = 6,
        NpcFile = 7,
        SpellFile = 8,
        AllPlayersList = 9,
        MapMutation = 10,
        FriendPlayersList = 11,
        ClassFile = 12,
        ErrorState = 0
    }
}
