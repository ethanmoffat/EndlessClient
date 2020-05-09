namespace EOLib.Domain.Account
{
    public class ChangePasswordParameters : IChangePasswordParameters
    {
        public string AccountName { get; }
        public string OldPassword { get; }
        public string NewPassword { get; }

        public ChangePasswordParameters(string accountName,
            string oldPassword,
            string newPassword)
        {
            AccountName = accountName;
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }
    }

    public interface IChangePasswordParameters
    {
        string AccountName { get; }
        string OldPassword { get; }
        string NewPassword { get; }
    }
}