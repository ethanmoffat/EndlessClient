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
            // implementation from Phorophor::notepad (thanks Blo)
            // https://discord.com/channels/723989119503696013/785190349026492437/791376941822246953

            // todo: make this use the authentic algorithm here: https://discord.com/channels/723989119503696013/787685796055482368/945700924536553544
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
