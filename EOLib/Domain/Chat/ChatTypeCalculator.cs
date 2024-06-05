using AutomaticTypeMapper;
using EOLib.Domain.Character;
using Moffat.EndlessOnline.SDK.Protocol;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
    public class ChatTypeCalculator : IChatTypeCalculator
    {
        private readonly ICharacterProvider _characterProvider;

        public ChatTypeCalculator(ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
        }

        public ChatType CalculateChatType(string input)
        {
            if(string.IsNullOrEmpty(input))
                return ChatType.Local;

            if(!CharacterIsAdmin && (input[0] == '+' || input[0] == '@'))
                return ChatType.Local;

            return input[0] switch
            {
                '+' => ChatType.Admin,
                '@' => ChatType.Announce,
                '\'' => ChatType.Party,
                '&' => ChatType.Guild,
                '~' => ChatType.Global,
                '!' => ChatType.PM,
                '#' => ChatType.Command,
                _ => ChatType.Local,
            };
        }

        private bool CharacterIsAdmin => _characterProvider.MainCharacter.AdminLevel != AdminLevel.Player;
    }

    public interface IChatTypeCalculator
    {
        ChatType CalculateChatType(string input);
    }
}