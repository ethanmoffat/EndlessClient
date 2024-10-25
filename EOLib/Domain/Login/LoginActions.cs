using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Extensions;
using EOLib.IO;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Login
{
    [AutoMappedType]
    public class LoginActions : ILoginActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterSelectorRepository _characterSelectorRepository;
        private readonly IPlayerInfoRepository _playerInfoRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ILoginFileChecksumRepository _loginFileChecksumRepository;
        private readonly INewsRepository _newsRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IPaperdollRepository _paperdollRepository;
        private readonly ICharacterSessionRepository _characterSessionRepository;

        public LoginActions(IPacketSendService packetSendService,
                            ICharacterSelectorRepository characterSelectorRepository,
                            IPlayerInfoRepository playerInfoRepository,
                            ICharacterRepository characterRepository,
                            ICurrentMapStateRepository currentMapStateRepository,
                            ILoginFileChecksumRepository loginFileChecksumRepository,
                            INewsRepository newsRepository,
                            ICharacterInventoryRepository characterInventoryRepository,
                            IPaperdollRepository paperdollRepository,
                            ICharacterSessionRepository characterSessionRepository)
        {
            _packetSendService = packetSendService;
            _characterSelectorRepository = characterSelectorRepository;
            _playerInfoRepository = playerInfoRepository;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _loginFileChecksumRepository = loginFileChecksumRepository;
            _newsRepository = newsRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _paperdollRepository = paperdollRepository;
            _characterSessionRepository = characterSessionRepository;
        }

        public bool LoginParametersAreValid(ILoginParameters parameters)
        {
            return !string.IsNullOrEmpty(parameters.Username) &&
                   !string.IsNullOrEmpty(parameters.Password);
        }

        public async Task<LoginReply> LoginToServer(ILoginParameters parameters)
        {
            var packet = new LoginRequestClientPacket
            {
                Username = parameters.Username,
                Password = parameters.Password,
            };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (IsInvalidResponse(response, out var responsePacket))
                throw new EmptyPacketReceivedException();

            if (responsePacket.ReplyCode == LoginReply.Ok)
            {
                _characterSelectorRepository.Characters = ((LoginReplyServerPacket.ReplyCodeDataOk)responsePacket.ReplyCodeData)
                    .Characters.Select(Character.Character.FromCharacterSelectionListEntry).ToList();
                _playerInfoRepository.LoggedInAccountName = parameters.Username;
                _playerInfoRepository.PlayerPassword = parameters.Password;
            }

            return responsePacket.ReplyCode;
        }

        public async Task<int> RequestCharacterLogin(Character.Character character)
        {
            var packet = new WelcomeRequestClientPacket { CharacterId = character.ID };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (IsInvalidWelcome(response, out var responsePacket) || responsePacket.WelcomeCode != WelcomeCode.SelectCharacter)
                throw new EmptyPacketReceivedException();

            var data = (WelcomeReplyServerPacket.WelcomeCodeDataSelectCharacter)responsePacket.WelcomeCodeData;

            _characterRepository.MainCharacter = character
                .WithID(data.CharacterId)
                .WithName(data.Name.Capitalize())
                .WithTitle(data.Title)
                .WithGuildName(data.GuildName.Capitalize())
                .WithGuildRank(data.GuildRankName.Capitalize())
                .WithGuildTag(data.GuildTag.ToUpper())
                .WithClassID(data.ClassId)
                .WithMapID(data.MapId)
                .WithAdminLevel(data.Admin)
                .WithStats(CharacterStats.FromSelectCharacterData(data));

            _playerInfoRepository.IsFirstTimePlayer = data.LoginMessageCode == LoginMessageCode.Yes;
            _playerInfoRepository.PlayerHasAdminCharacter = _characterSelectorRepository.Characters.Any(x => x.AdminLevel > 0);

            _currentMapStateRepository.CurrentMapID = data.MapId;
            _currentMapStateRepository.JailMapID = data.Settings.JailMap;

            _paperdollRepository.VisibleCharacterPaperdolls[data.SessionId] = new PaperdollData()
                .WithName(data.Name.Capitalize())
                .WithTitle(data.Title)
                .WithGuild(data.GuildName.Capitalize())
                .WithRank(data.GuildRankName.Capitalize())
                .WithClass(data.ClassId)
                .WithPlayerID(data.SessionId)
                .WithPaperdoll(data.Equipment.GetPaperdoll());

            _loginFileChecksumRepository.MapChecksum = data.MapRid;
            _loginFileChecksumRepository.MapLength = data.MapFileSize;

            _loginFileChecksumRepository.EIFChecksum = data.EifRid;
            _loginFileChecksumRepository.EIFLength = data.EifLength;
            _loginFileChecksumRepository.ENFChecksum = data.EnfRid;
            _loginFileChecksumRepository.ENFLength = data.EnfLength;
            _loginFileChecksumRepository.ESFChecksum = data.EsfRid;
            _loginFileChecksumRepository.ESFLength = data.EsfLength;
            _loginFileChecksumRepository.ECFChecksum = data.EcfRid;
            _loginFileChecksumRepository.ECFLength = data.EcfLength;

            return data.SessionId;
        }

        public async Task<WelcomeCode> CompleteCharacterLogin(int sessionID)
        {
            var packet = new WelcomeMsgClientPacket
            {
                SessionId = sessionID,
                CharacterId = _characterRepository.MainCharacter.ID
            };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (IsInvalidWelcome(response, out var responsePacket))
                throw new EmptyPacketReceivedException();

            if (responsePacket.WelcomeCode != WelcomeCode.EnterGame)
                return responsePacket.WelcomeCode;

            var data = (WelcomeReplyServerPacket.WelcomeCodeDataEnterGame)responsePacket.WelcomeCodeData;

            _newsRepository.NewsHeader = data.News.First();
            _newsRepository.NewsText = data.News.Skip(1).ToList();

            var mainCharacter = data.Nearby.Characters.Single(
                x => x.Name.Equals(_characterRepository.MainCharacter.Name, StringComparison.OrdinalIgnoreCase));

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, data.Weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, data.Weight.Max)
                .WithNewStat(CharacterStat.Level, mainCharacter.Level)
                .WithNewStat(CharacterStat.HP, mainCharacter.Hp)
                .WithNewStat(CharacterStat.MaxHP, mainCharacter.MaxHp)
                .WithNewStat(CharacterStat.TP, mainCharacter.Tp)
                .WithNewStat(CharacterStat.MaxTP, mainCharacter.MaxTp);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithID(_playerInfoRepository.PlayerID)
                .WithName(char.ToUpper(mainCharacter.Name[0]) + mainCharacter.Name.Substring(1))
                .WithMapID(mainCharacter.MapId)
                .WithGuildTag(mainCharacter.GuildTag)
                // classId is sent in the "enter game" data but eoserv hard-codes this to 6 (with no apparent impact on game function for main player)
                //.WithClassID(mainCharacter.ClassId) 
                .WithStats(stats)
                .WithRenderProperties(CharacterRenderProperties.FromCharacterMapInfo(mainCharacter));

            _characterInventoryRepository.ItemInventory = new HashSet<InventoryItem>(data.Items.Select(InventoryItem.FromNet));
            if (!_characterInventoryRepository.ItemInventory.Any(x => x.ItemID == 1))
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, 0));

            _characterInventoryRepository.SpellInventory = new HashSet<InventorySpell>(data.Spells.Select(InventorySpell.FromNet));

            _currentMapStateRepository.Characters = new MapEntityCollectionHashSet<Character.Character>(
                c => c.ID,
                c => new MapCoordinate(c.X, c.Y),
                data.Nearby.Characters
                    .Where(x => x.ByteSize >= 42 && x.PlayerId != mainCharacter.PlayerId)
                    .Select(Character.Character.FromNearby)
            );
            _currentMapStateRepository.NPCs = new MapEntityCollectionHashSet<NPC.NPC>(
                n => n.Index,
                n => new MapCoordinate(n.X, n.Y),
                data.Nearby.Npcs.Select(NPC.NPC.FromNearby)
            );
            _currentMapStateRepository.MapItems = new MapEntityCollectionHashSet<MapItem>(
                item => item.UniqueID,
                item => new MapCoordinate(item.X, item.Y),
                data.Nearby.Items.Select(MapItem.FromNearby)
            );

            _playerInfoRepository.PlayerIsInGame = true;
            _characterSessionRepository.ResetState();

            return WelcomeCode.EnterGame;
        }

        private static bool IsInvalidResponse(IPacket response, out LoginReplyServerPacket responsePacket)
        {
            responsePacket = response as LoginReplyServerPacket;
            if (responsePacket == null && response is InitInitServerPacket initPacket && initPacket.ReplyCode == InitReply.Banned)
            {
                responsePacket = new LoginReplyServerPacket
                {
                    ReplyCode = LoginReply.Banned,
                    ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataBanned()
                };
                return false;
            }

            return !(response.Family == PacketFamily.Login && response.Action == PacketAction.Reply);
        }

        private static bool IsInvalidWelcome(IPacket response, out WelcomeReplyServerPacket responsePacket)
        {
            responsePacket = response as WelcomeReplyServerPacket;
            return response.Family != PacketFamily.Welcome || response.Action != PacketAction.Reply;
        }
    }

    public interface ILoginActions
    {
        bool LoginParametersAreValid(ILoginParameters parameters);

        Task<LoginReply> LoginToServer(ILoginParameters parameters);

        Task<int> RequestCharacterLogin(Character.Character character);

        Task<WelcomeCode> CompleteCharacterLogin(int sessionID);
    }
}
