namespace EOLib.Domain.Character
{
    public enum CharacterReply : short
    {
        Exists = 1,
        Full = 2,
        NotApproved = 4,
        Ok = 5,
        Deleted = 6,
        THIS_IS_WRONG = 255
    }
}