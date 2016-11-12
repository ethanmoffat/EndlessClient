// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Translators;

namespace EOLib.Domain.Login
{
    public class AccountLoginData : IAccountLoginData
    {
        public LoginReply Response { get; private set; }

        private readonly List<ICharacter> _characters;
        public IReadOnlyList<ICharacter> Characters { get { return _characters; } }

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
