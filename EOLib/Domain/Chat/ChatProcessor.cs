using System;
using System.Linq;
using System.Text;
using AutomaticTypeMapper;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
    public class ChatProcessor : IChatProcessor
    {
        private readonly Random _random = new Random();

        public string RemoveFirstCharacterIfNeeded(string chat, ChatType chatType, string targetCharacter)
        {
            switch (chatType)
            {
                case ChatType.Command:
                case ChatType.NPC:
                case ChatType.Server:
                    throw new ArgumentOutOfRangeException(nameof(chatType));
                case ChatType.Admin:
                case ChatType.Global:
                case ChatType.Guild:
                case ChatType.Party:
                case ChatType.Announce:
                    return chat.Substring(1);
                case ChatType.PM:
                    chat = chat.Substring(1);
                    //todo: need to just send the whole string if the selected tab is the target character
                    //currently this is incorrect since it will remove the name
                    if (chat.ToLower().StartsWith(targetCharacter.ToLower()))
                        chat = chat.Substring(targetCharacter.Length);
                    return chat;
                case ChatType.Local:
                    return chat;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chatType));
            }
        }

        public string MakeDrunk(string input)
        {
            // implementation from Phorophor::notepad (thanks Blo)
            // https://discord.com/channels/723989119503696013/785190349026492437/791376941822246953
            var ret = new StringBuilder();

            foreach (var c in input)
            {
                var repeats = _random.Next(0, 8) < 6 ? 1 : 2;
                ret.Append(c, repeats);

                if ((c == 'a' || c == 'e') && _random.NextDouble() / 1.0 < 0.555)
                    ret.Append('j');

                if ((c == 'u' || c == 'o') && _random.NextDouble() / 1.0 < 0.444)
                    ret.Append('w');

                if ((c == ' ') && _random.NextDouble() / 1.0 < 0.333)
                    ret.Append(" *hic*");
            }

            return ret.ToString();
        }
    }

    public interface IChatProcessor
    {
        string RemoveFirstCharacterIfNeeded(string input, ChatType chatType, string targetCharacter);

        string MakeDrunk(string input);
    }
}
