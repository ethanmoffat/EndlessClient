namespace EOLib.Domain.Login
{
    public enum LoginReply : short
    {
        WrongUser = 1,
        WrongUserPass = 2,
        Ok = 3,
        AccountBanned = 4,
        LoggedIn = 5,
        Busy = 6,
        THIS_IS_WRONG = 255
    }
}