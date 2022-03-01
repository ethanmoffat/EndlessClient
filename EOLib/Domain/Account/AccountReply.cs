namespace EOLib.Domain.Account
{
    public enum AccountReply : ushort
    {
        THIS_IS_WRONG = 0,
        Exists = 1,
        NotApproved = 2,
        Created = 3,
        ChangeFailed = 5,
        ChangeSuccess = 6,
        /// <summary>
        /// Anything greater or equal to this value means the account was approved
        /// </summary>
        OK_CodeRange = 10,
    }
}