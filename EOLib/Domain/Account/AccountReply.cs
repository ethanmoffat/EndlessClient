namespace EOLib.Domain.Account
{
    public enum AccountReply : short
    {
        THIS_IS_WRONG = 0,
        Exists = 1,
        NotApproved = 2,
        Created = 3,
        ChangeFailed = 5,
        ChangeSuccess = 6,
        Continue = 1000
    }
}