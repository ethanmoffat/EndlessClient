namespace EOLib.Domain.Login
{
    public class LoginParameters : ILoginParameters
    {
        public string Username { get; }
        public string Password { get; }

        public LoginParameters(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public interface ILoginParameters
    {
        string Username { get; }
        string Password { get; }
    }
}