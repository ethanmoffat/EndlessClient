using System;
using System.Linq;
using System.Text;
using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Localization;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
    public class ChatProcessor : IChatProcessor
    {
        private readonly Random _random = new Random();

        private readonly IDataFileProvider _dataFileProvider;
        private readonly IConfigurationProvider _configurationProvider;

        public ChatProcessor(IDataFileProvider dataFileProvider,
                             IConfigurationProvider configurationProvider)
        {
            _dataFileProvider = dataFileProvider;
            _configurationProvider = configurationProvider;
        }

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
            // algorithm from: https://discord.com/channels/723989119503696013/787685796055482368/945700924536553544
            var ret = new StringBuilder(input);

            //Pass 1:
            //If E or A: 70 % chance to insert a j
            //if U or O: 70 % chance to insert a w
            //if i(lowercase only): 40 % chance to insert a u
            //if not a space: 40 % chance to double the letter
            for (int i = 0; i < ret.Length; i++)
            {
                var c = ret[i];

                if (char.ToLower(c) == 'e' || char.ToLower(c) == 'a')
                {
                    if (_random.Next(100) < 70)
                        ret.Insert(i+1, 'j');
                }
                else if (char.ToLower(c) == 'u' || char.ToLower(c) == 'o')
                {
                    if (_random.Next(100) < 70)
                        ret.Insert(i+1, 'w');
                }
                else if (c == 'i')
                {
                    if (_random.Next(100) < 40)
                        ret.Insert(i+1, 'u');
                }
                else if (c != ' ' && _random.Next(100) < 40)
                {
                    ret.Insert(i+1, c);
                }
            }


            //Pass 2:
            //if vowel: 1 in 12 chance to replace with *
            for (int i = 0; i < ret.Length; i++)
            {
                var c = char.ToLower(ret[i]);

                if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u')
                {
                    if (_random.Next(12) == 6)
                        ret[i] = '*';
                }
            }

            //Pass 3:
            //if space, 30 % chance to insert "hic"
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i] == ' ' && _random.Next(100) < 30)
                {
                    ret.Insert(i+1, "*hic* ");
                    i += 6;
                }
            }

            // If string length > 128: trim to 126 bytes and add ".."
            if (ret.Length > 128)
            {
                ret = ret.Remove(126, ret.Length - 126);
                ret.Append("..");
            }

            return ret.ToString();
        }

        public (bool, string) FilterCurses(string input)
        {
            if (_configurationProvider.CurseFilterEnabled || _configurationProvider.StrictFilterEnabled)
            {
                foreach (var curse in _dataFileProvider.DataFiles[DataFiles.CurseFilter].Data.Values)
                {
                    var index = input.IndexOf(curse, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        if (_configurationProvider.CurseFilterEnabled)
                        {
                            input = input.Remove(index, curse.Length);
                            input = input.Insert(index, "****");
                        }
                        else if (_configurationProvider.StrictFilterEnabled)
                        {
                            return (false, string.Empty);
                        }
                    }
                }
            }

            return (true, input);
        }
    }

    public interface IChatProcessor
    {
        string RemoveFirstCharacterIfNeeded(string input, ChatType chatType, string targetCharacter);

        string MakeDrunk(string input);

        (bool ShowChat, string FilteredMessage) FilterCurses(string input);
    }
}
