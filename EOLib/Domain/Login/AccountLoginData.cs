using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Login
{
    public class AccountLoginData : IAccountLoginData
    {
        public LoginReply Response { get; }

        private readonly List<Character.Character> _characters;
        public IReadOnlyList<Character.Character> Characters => _characters;

        public AccountLoginData(LoginReply reply, List<Character.Character> characters)
        {
            Response = reply;
            _characters = characters;
        }
    }

    public interface IAccountLoginData : ITranslatedData
    {
        LoginReply Response { get; }

        IReadOnlyList<Character.Character> Characters { get; }
    }
}
