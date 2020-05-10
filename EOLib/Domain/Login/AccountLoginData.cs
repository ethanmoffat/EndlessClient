using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Translators;

namespace EOLib.Domain.Login
{
    public class AccountLoginData : IAccountLoginData
    {
        public LoginReply Response { get; }

        private readonly List<ICharacter> _characters;
        public IReadOnlyList<ICharacter> Characters => _characters;

        public AccountLoginData(LoginReply reply, List<ICharacter> characters)
        {
            Response = reply;
            _characters = characters;
        }
    }

    public interface IAccountLoginData : ITranslatedData
    {
        LoginReply Response { get; }

        IReadOnlyList<ICharacter> Characters { get; }
    }
}
