﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Domain.Protocol;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using EOLib.Net.Translators;

namespace EOLib.Domain.Login
{
    [AutoMappedType]
    public class LoginActions : ILoginActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IPacketTranslator<IAccountLoginData> _loginPacketTranslator;
        private readonly IPacketTranslator<ILoginRequestGrantedData> _loginRequestGrantedPacketTranslator;
        private readonly IPacketTranslator<ILoginRequestCompletedData> _loginRequestCompletedPacketTranslator;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICharacterSelectorRepository _characterSelectorRepository;
        private readonly IPlayerInfoRepository _playerInfoRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ILoginFileChecksumRepository _loginFileChecksumRepository;
        private readonly INewsRepository _newsRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IPaperdollRepository _paperdollRepository;

        public LoginActions(IPacketSendService packetSendService,
                            IPacketTranslator<IAccountLoginData> loginPacketTranslator,
                            IPacketTranslator<ILoginRequestGrantedData> loginRequestGrantedPacketTranslator,
                            IPacketTranslator<ILoginRequestCompletedData> loginRequestCompletedPacketTranslator,
                            ILocalizedStringFinder localizedStringFinder,
                            ICharacterSelectorRepository characterSelectorRepository,
                            IPlayerInfoRepository playerInfoRepository,
                            ICharacterRepository characterRepository,
                            ICurrentMapStateRepository currentMapStateRepository,
                            ILoginFileChecksumRepository loginFileChecksumRepository,
                            INewsRepository newsRepository,
                            IChatRepository chatRepository,
                            ICharacterInventoryRepository characterInventoryRepository,
                            IPaperdollRepository paperdollRepository)
        {
            _packetSendService = packetSendService;
            _loginPacketTranslator = loginPacketTranslator;
            _loginRequestGrantedPacketTranslator = loginRequestGrantedPacketTranslator;
            _loginRequestCompletedPacketTranslator = loginRequestCompletedPacketTranslator;
            _localizedStringFinder = localizedStringFinder;
            _characterSelectorRepository = characterSelectorRepository;
            _playerInfoRepository = playerInfoRepository;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _loginFileChecksumRepository = loginFileChecksumRepository;
            _newsRepository = newsRepository;
            _chatRepository = chatRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _paperdollRepository = paperdollRepository;
        }

        public bool LoginParametersAreValid(ILoginParameters parameters)
        {
            return !string.IsNullOrEmpty(parameters.Username) &&
                   !string.IsNullOrEmpty(parameters.Password);
        }

        public async Task<LoginReply> LoginToServer(ILoginParameters parameters)
        {
            var packet = new PacketBuilder(PacketFamily.Login, PacketAction.Request)
                .AddBreakString(parameters.Username)
                .AddBreakString(parameters.Password)
                .Build();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (IsInvalidResponse(response))
                throw new EmptyPacketReceivedException();

            var data = _loginPacketTranslator.TranslatePacket(response);
            _characterSelectorRepository.Characters = data.Characters;

            if (data.Response == LoginReply.Ok)
            {
                _playerInfoRepository.LoggedInAccountName = parameters.Username;
                _playerInfoRepository.PlayerPassword = parameters.Password;
            }

            return data.Response;
        }

        public async Task RequestCharacterLogin(ICharacter character)
        {
            var packet = new PacketBuilder(PacketFamily.Welcome, PacketAction.Request)
                .AddInt(character.ID)
                .Build();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (IsInvalidWelcome(response))
                throw new EmptyPacketReceivedException();

            var data = _loginRequestGrantedPacketTranslator.TranslatePacket(response);

            _characterRepository.MainCharacter = character
                .WithID(data.CharacterID)
                .WithName(data.Name)
                .WithTitle(data.Title)
                .WithGuildName(data.GuildName)
                .WithGuildRank(data.GuildRank)
                .WithGuildTag(data.GuildTag)
                .WithClassID(data.ClassID)
                .WithMapID(data.MapID)
                .WithAdminLevel(data.AdminLevel)
                .WithStats(data.CharacterStats);
            _paperdollRepository.MainCharacterPaperdoll = data.Paperdoll.ToList();

            _playerInfoRepository.PlayerID = data.PlayerID;
            _playerInfoRepository.IsFirstTimePlayer = data.FirstTimePlayer;
            _currentMapStateRepository.CurrentMapID = data.MapID;

            _loginFileChecksumRepository.MapChecksum = data.MapRID.ToArray();
            _loginFileChecksumRepository.MapLength = data.MapLen;

            _loginFileChecksumRepository.EIFChecksum = data.EifRid;
            _loginFileChecksumRepository.EIFLength = data.EifLen;
            _loginFileChecksumRepository.ENFChecksum = data.EnfRid;
            _loginFileChecksumRepository.ENFLength = data.EnfLen;
            _loginFileChecksumRepository.ESFChecksum = data.EsfRid;
            _loginFileChecksumRepository.ESFLength = data.EsfLen;
            _loginFileChecksumRepository.ECFChecksum = data.EcfRid;
            _loginFileChecksumRepository.ECFLength = data.EcfLen;
        }

        public async Task CompleteCharacterLogin()
        {
            var packet = new PacketBuilder(PacketFamily.Welcome, PacketAction.Message)
                .AddThree(0x00123456) //?
                .AddInt(_characterRepository.MainCharacter.ID)
                .Build();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (IsInvalidWelcome(response))
                throw new EmptyPacketReceivedException();

            var data = _loginRequestCompletedPacketTranslator.TranslatePacket(response);

            _newsRepository.NewsHeader = data.News.First();
            _newsRepository.NewsText = data.News.Except(new[] { data.News.First() }).ToList();
            AddDefaultTextToChat();

            var mainCharacter = data.MapCharacters.Single(
                x => x.Name.ToLower() == _characterRepository.MainCharacter.Name.ToLower());

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, data.CharacterWeight)
                .WithNewStat(CharacterStat.MaxWeight, data.CharacterMaxWeight)
                .WithNewStat(CharacterStat.Level, mainCharacter.Stats[CharacterStat.Level])
                .WithNewStat(CharacterStat.HP, mainCharacter.Stats[CharacterStat.HP])
                .WithNewStat(CharacterStat.MaxHP, mainCharacter.Stats[CharacterStat.MaxHP])
                .WithNewStat(CharacterStat.TP, mainCharacter.Stats[CharacterStat.TP])
                .WithNewStat(CharacterStat.MaxTP, mainCharacter.Stats[CharacterStat.MaxTP]);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithID(mainCharacter.ID)
                .WithName(mainCharacter.Name)
                .WithMapID(mainCharacter.MapID)
                .WithGuildTag(mainCharacter.GuildTag)
                .WithStats(stats)
                .WithRenderProperties(mainCharacter.RenderProperties);

            _characterInventoryRepository.ItemInventory = data.CharacterItemInventory.ToList();
            _characterInventoryRepository.SpellInventory = data.CharacterSpellInventory.ToList();

            _currentMapStateRepository.Characters = new HashSet<ICharacter>(data.MapCharacters.Except(new[] { mainCharacter }));
            _currentMapStateRepository.NPCs = new HashSet<INPC>(data.MapNPCs);
            _currentMapStateRepository.MapItems = new HashSet<IItem>(data.MapItems);

            _playerInfoRepository.PlayerIsInGame = true;
        }

        private bool IsInvalidResponse(IPacket response)
        {
            return !(response.Family == PacketFamily.Login && response.Action == PacketAction.Reply)
                && !(response.Family == PacketFamily.Init && response.Action == PacketAction.Init && response.PeekByte() == (byte)InitReply.BannedFromServer);
        }

        private bool IsInvalidWelcome(IPacket response)
        {
            return response.Family != PacketFamily.Welcome || response.Action != PacketAction.Reply;
        }

        private void AddDefaultTextToChat()
        {
            var server = _localizedStringFinder.GetString(EOResourceID.STRING_SERVER);
            var serverMessage1 = _localizedStringFinder.GetString(EOResourceID.GLOBAL_CHAT_SERVER_MESSAGE_1);
            var serverMessage2 = _localizedStringFinder.GetString(EOResourceID.GLOBAL_CHAT_SERVER_MESSAGE_2);

            _chatRepository.AllChat[ChatTab.Local].Add(
                new ChatData(server, _newsRepository.NewsHeader, ChatIcon.Note, ChatColor.Server));
            _chatRepository.AllChat[ChatTab.Global].Add(
                new ChatData(server, serverMessage1, ChatIcon.Note, ChatColor.Server));
            _chatRepository.AllChat[ChatTab.Global].Add(
                new ChatData(server, serverMessage2, ChatIcon.Note, ChatColor.Server));
        }
    }

    public interface ILoginActions
    {
        bool LoginParametersAreValid(ILoginParameters parameters);

        Task<LoginReply> LoginToServer(ILoginParameters parameters);

        Task RequestCharacterLogin(ICharacter character);

        Task CompleteCharacterLogin();
    }
}