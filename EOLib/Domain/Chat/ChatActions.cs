using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat.Builders;
using EOLib.Domain.Map;
using EOLib.Domain.Party;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using System;
using System.Linq;

namespace EOLib.Domain.Chat
{
    public enum ChatResult
    {
        Ok,
        YourMindPrevents,
        HideSpeechBubble,
        Command,
        AdminAnnounce,
        HideAll,
        JailProtection
    }

    [AutoMappedType]
    public class ChatActions : IChatActions
    {
        private readonly IChatRepository _chatRepository;
        private readonly ICharacterProvider _characterProvider;
        private readonly IPartyDataProvider _partyDataProvider;
        private readonly IChatPacketBuilder _chatPacketBuilder;
        private readonly IPacketSendService _packetSendService;
        private readonly ILocalCommandHandler _localCommandHandler;
        private readonly IChatProcessor _chatProcessor;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public ChatActions(IChatRepository chatRepository,
                           ICharacterProvider characterProvider,
                           IPartyDataProvider partyDataProvider,
                           IChatPacketBuilder chatPacketBuilder,
                           IPacketSendService packetSendService,
                           ILocalCommandHandler localCommandHandler,
                           IChatProcessor chatProcessor,
                           ICurrentMapStateProvider currentMapStateProvider)
        {
            _chatRepository = chatRepository;
            _characterProvider = characterProvider;
            _partyDataProvider = partyDataProvider;
            _chatPacketBuilder = chatPacketBuilder;
            _packetSendService = packetSendService;
            _localCommandHandler = localCommandHandler;
            _chatProcessor = chatProcessor;
            _currentMapStateProvider = currentMapStateProvider;
        }

        public (ChatResult, string) SendChatToServer(string chat, string targetCharacter, ChatType chatType)
        {
            if (chatType == ChatType.Command)
            {
                if (HandleCommand(chat))
                    return (ChatResult.Command, chat);

                //treat unhandled command as local chat
                chatType = ChatType.Local;
            }
            else if (chatType == ChatType.PM)
            {
                if (string.IsNullOrEmpty(_chatRepository.PMTarget1))
                    _chatRepository.PMTarget1 = targetCharacter;
                else if (string.IsNullOrEmpty(_chatRepository.PMTarget2))
                    _chatRepository.PMTarget2 = targetCharacter;
            }
            else if (chatType == ChatType.Party && !_partyDataProvider.Members.Any())
            {
                return (ChatResult.HideAll, string.Empty);
            }
            else if (chatType == ChatType.Global && _currentMapStateProvider.IsJail)
            {
                return (ChatResult.JailProtection, string.Empty);
            }

            chat = _chatProcessor.RemoveFirstCharacterIfNeeded(chat, chatType, targetCharacter);
            var (ok, filtered) = _chatProcessor.FilterCurses(chat);
            if (!ok)
            {
                return (ChatResult.YourMindPrevents, filtered);
            }

            chat = filtered;

            if (_characterProvider.MainCharacter.RenderProperties.IsDrunk)
                chat = _chatProcessor.MakeDrunk(chat);

            var chatPacket = _chatPacketBuilder.BuildChatPacket(chatType, chat, targetCharacter);
            _packetSendService.SendPacket(chatPacket);

            AddChatForLocalDisplay(chatType, chat, targetCharacter);

            return (chatType switch
            {
                ChatType.PM => ChatResult.HideSpeechBubble,
                ChatType.Global => ChatResult.HideSpeechBubble,
                ChatType.Admin => ChatResult.HideSpeechBubble,
                ChatType.Announce => ChatResult.AdminAnnounce,
                _ => ChatResult.Ok,
            }, chat);
        }

        public void SetHearWhispers(bool whispersEnabled)
        {
            var packet = whispersEnabled ? (IPacket)new GlobalRemoveClientPacket() : new GlobalPlayerClientPacket();
            _packetSendService.SendPacket(packet);
        }

        public void SetGlobalActive(bool active)
        {
            var packet = active ? (IPacket)new GlobalOpenClientPacket() : new GlobalCloseClientPacket();
            _packetSendService.SendPacket(packet);
        }

        public void ClosePMTab(ChatTab whichTab)
        {
            if (whichTab != ChatTab.Private1 && whichTab != ChatTab.Private2)
                throw new ArgumentException("Tab must be a PM tab", nameof(whichTab));

            _chatRepository.AllChat[whichTab].Clear();

            if (whichTab == ChatTab.Private1)
                _chatRepository.PMTarget1 = null;
            else if (whichTab == ChatTab.Private2)
                _chatRepository.PMTarget2 = null;
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
                    if (targetCharacter == _chatRepository.PMTarget1)
                        _chatRepository.AllChat[ChatTab.Private1].Add(new ChatData(ChatTab.Private1, who, chat, ChatIcon.Note, ChatColor.PM));
                    else if (targetCharacter == _chatRepository.PMTarget2)
                        _chatRepository.AllChat[ChatTab.Private2].Add(new ChatData(ChatTab.Private2, who, chat, ChatIcon.Note, ChatColor.PM));

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
        (ChatResult Ok, string Processed) SendChatToServer(string chat, string targetCharacter, ChatType chatType);

        void SetHearWhispers(bool whispersEnabled);

        void SetGlobalActive(bool active);

        void ClosePMTab(ChatTab tab);
    }
}