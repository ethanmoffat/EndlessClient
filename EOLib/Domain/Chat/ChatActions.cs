using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Net;
using EOLib.Net.Builders;
using EOLib.Net.Communication;
using System;
using System.Linq;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
    public class ChatActions : IChatActions
    {
        private readonly IChatRepository _chatRepository;
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatTypeCalculator _chatTypeCalculator;
        private readonly IChatPacketBuilder _chatPacketBuilder;
        private readonly IPacketSendService _packetSendService;
        private readonly ILocalCommandHandler _localCommandHandler;
        private readonly IChatProcessor _chatProcessor;
        private readonly IConfigurationProvider _configurationProvider;

        public ChatActions(IChatRepository chatRepository,
                           ICharacterProvider characterProvider,
                           IChatTypeCalculator chatTypeCalculator,
                           IChatPacketBuilder chatPacketBuilder,
                           IPacketSendService packetSendService,
                           ILocalCommandHandler localCommandHandler,
                           IChatProcessor chatProcessor,
                           IConfigurationProvider configurationProvider)
        {
            _chatRepository = chatRepository;
            _characterProvider = characterProvider;
            _chatTypeCalculator = chatTypeCalculator;
            _chatPacketBuilder = chatPacketBuilder;
            _packetSendService = packetSendService;
            _localCommandHandler = localCommandHandler;
            _chatProcessor = chatProcessor;
            _configurationProvider = configurationProvider;
        }

        public (bool, string) SendChatToServer(string chat, string targetCharacter)
        {
            var chatType = _chatTypeCalculator.CalculateChatType(chat);

            if (chatType == ChatType.Command)
            {
                if (HandleCommand(chat))
                    return (true, chat);

                //treat unhandled command as local chat
                chatType = ChatType.Local;
            }

            if (chatType == ChatType.PM)
            {
                if (string.IsNullOrEmpty(_chatRepository.PMTarget1))
                    _chatRepository.PMTarget1 = targetCharacter;
                else if (string.IsNullOrEmpty(_chatRepository.PMTarget2))
                    _chatRepository.PMTarget2 = targetCharacter;
            }

            chat = _chatProcessor.RemoveFirstCharacterIfNeeded(chat, chatType, targetCharacter);
            var (ok, filtered) = _chatProcessor.FilterCurses(chat);
            if (!ok)
            {
                return (ok, filtered);
            }

            chat = filtered;

            if (_characterProvider.MainCharacter.RenderProperties.IsDrunk)
                chat = _chatProcessor.MakeDrunk(chat);

            var chatPacket = _chatPacketBuilder.BuildChatPacket(chatType, chat, targetCharacter);
            _packetSendService.SendPacket(chatPacket);

            AddChatForLocalDisplay(chatType, chat, targetCharacter);

            return (ok, chat);
        }

        public void SetHearWhispers(bool whispersEnabled)
        {
            // GLOBAL_REMOVE with 'n' enables whispers...? 
            var packet = new PacketBuilder(PacketFamily.Global, whispersEnabled ? PacketAction.Remove : PacketAction.Player)
                .AddChar((byte)(whispersEnabled ? 'n' : 'y'))
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void SetGlobalActive(bool active)
        {
            var packet = new PacketBuilder(PacketFamily.Global, active ? PacketAction.Open : PacketAction.Close)
                .AddChar((byte)(active ? 'y' : 'n'))
                .Build();
            _packetSendService.SendPacket(packet);
        }

        /// <summary>
        /// Returns true if the text is a command (#) and can be handled as such, false if it should be sent to the server as a public chat string
        /// </summary>
        private bool HandleCommand(string commandString)
        {
            var command = new string(commandString.Substring(1)
                .TakeWhile(x => x != ' ')
                .ToArray())
                .Trim();
            var parameters = new string(commandString.SkipWhile(x => x != ' ')
                .ToArray())
                .Trim();
            return _localCommandHandler.HandleCommand(command, parameters);
        }

        private void AddChatForLocalDisplay(ChatType chatType, string chat, string targetCharacter)
        {
            var who = _characterProvider.MainCharacter.Name;
            switch (chatType)
            {
                case ChatType.Admin:
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(ChatTab.Group, who, chat, ChatIcon.HGM, ChatColor.Admin));
                    break;
                case ChatType.PM:
                    if(targetCharacter == _chatRepository.PMTarget1)
                        _chatRepository.AllChat[ChatTab.Private1].Add(new ChatData(ChatTab.Private1, who, chat, ChatIcon.Note, ChatColor.PM));
                    else if (targetCharacter == _chatRepository.PMTarget2)
                        _chatRepository.AllChat[ChatTab.Private2].Add(new ChatData(ChatTab.Private2, who, chat, ChatIcon.Note, ChatColor.PM));
                    else
                        throw new ArgumentException("Unexpected target character!", nameof(targetCharacter));

                    break;
                case ChatType.Local:
                    _chatRepository.AllChat[ChatTab.Local].Add(new ChatData(ChatTab.Local, who, chat));
                    break;
                case ChatType.Global:
                    _chatRepository.AllChat[ChatTab.Global].Add(new ChatData(ChatTab.Global, who, chat));
                    break;
                case ChatType.Guild:
                    //todo: there are special cases here for guild chat that aren't handled
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(ChatTab.Group, who, chat));
                    break;
                case ChatType.Party:
                    _chatRepository.AllChat[ChatTab.Local].Add(new ChatData(ChatTab.Local, who, chat, ChatIcon.PlayerPartyDark, ChatColor.PM, log: false));
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(ChatTab.Group, who, chat, ChatIcon.PlayerPartyDark));
                    break;
                case ChatType.Announce:
                    _chatRepository.AllChat[ChatTab.Local].Add(new ChatData(ChatTab.Local, who, chat, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal, log: false));
                    _chatRepository.AllChat[ChatTab.Global].Add(new ChatData(ChatTab.Global, who, chat, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal));
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(ChatTab.Group, who, chat, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal, log: false));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chatType), chatType, "Unexpected ChatType encountered");
            }
        }
    }

    public interface IChatActions
    {
        (bool Ok, string Processed) SendChatToServer(string chat, string targetCharacter);

        void SetHearWhispers(bool whispersEnabled);

        void SetGlobalActive(bool active);
    }
}