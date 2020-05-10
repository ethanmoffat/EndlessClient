using System.IO;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.IO;
using EOLib.IO.Services;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class LoginRequestGrantedPacketTranslator : IPacketTranslator<ILoginRequestGrantedData>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public LoginRequestGrantedPacketTranslator(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public ILoginRequestGrantedData TranslatePacket(IPacket packet)
        {
            var reply = (CharacterLoginReply)packet.ReadShort();
            if (reply != CharacterLoginReply.RequestGranted)
                throw new MalformedPacketException("Unexpected welcome response in packet: " + reply, packet);

            var playerID = packet.ReadShort();
            var characterID = packet.ReadInt();

            var mapID = packet.ReadShort();
            var mapRid = packet.ReadBytes(4).ToArray();
            var mapLen = packet.ReadThree();

            var eifRid = packet.ReadInt();
            var eifLen = packet.ReadShort();

            var enfRid = packet.ReadInt();
            var enfLen = packet.ReadShort();

            var esfRid = packet.ReadInt();
            var esfLen = packet.ReadShort();

            var ecfRid = packet.ReadInt();
            var ecfLen = packet.ReadShort();

            var characterName = packet.ReadBreakString();
            var characterTitle = packet.ReadBreakString();
            var guildName = packet.ReadBreakString();
            var guildRank = packet.ReadBreakString();
            var classID = packet.ReadChar();
            var paddedGuildTag = packet.ReadString(3); //padded guild tag is 3 characters
            var adminLevel = (AdminLevel)packet.ReadChar();

            var level = packet.ReadChar();
            var exp = packet.ReadInt();
            var usage = packet.ReadInt();

            var hp = packet.ReadShort();
            var maxHP = packet.ReadShort();
            var tp = packet.ReadShort();
            var maxTP = packet.ReadShort();
            var maxSP = packet.ReadShort();

            var statPoints = packet.ReadShort();
            var skillPoints = packet.ReadShort();
            var karma = packet.ReadShort();
            var minDam = packet.ReadShort();
            var maxDam = packet.ReadShort();
            var accuracy = packet.ReadShort();
            var evade = packet.ReadShort();
            var armor = packet.ReadShort();
            var dispStr = packet.ReadShort();
            var dispInt = packet.ReadShort();
            var dispWis = packet.ReadShort();
            var dispAgi = packet.ReadShort();
            var dispCon = packet.ReadShort();
            var dispCha = packet.ReadShort();

            var characterStats = new CharacterStats()
                .WithNewStat(CharacterStat.Level, level)
                .WithNewStat(CharacterStat.Experience, exp)
                .WithNewStat(CharacterStat.Usage, usage)
                .WithNewStat(CharacterStat.HP, hp)
                .WithNewStat(CharacterStat.MaxHP, maxHP)
                .WithNewStat(CharacterStat.TP, tp)
                .WithNewStat(CharacterStat.MaxTP, maxTP)
                .WithNewStat(CharacterStat.SP, maxSP)
                .WithNewStat(CharacterStat.MaxSP, maxSP)
                .WithNewStat(CharacterStat.StatPoints, statPoints)
                .WithNewStat(CharacterStat.SkillPoints, skillPoints)
                .WithNewStat(CharacterStat.Karma, karma)
                .WithNewStat(CharacterStat.MinDam, minDam)
                .WithNewStat(CharacterStat.MaxDam, maxDam)
                .WithNewStat(CharacterStat.Accuracy, accuracy)
                .WithNewStat(CharacterStat.Evade, evade)
                .WithNewStat(CharacterStat.Armor, armor)
                .WithNewStat(CharacterStat.Strength, dispStr)
                .WithNewStat(CharacterStat.Intelligence, dispInt)
                .WithNewStat(CharacterStat.Wisdom, dispWis)
                .WithNewStat(CharacterStat.Agility, dispAgi)
                .WithNewStat(CharacterStat.Constituion, dispCon)
                .WithNewStat(CharacterStat.Charisma, dispCha);

            var paperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
            for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
            {
                paperDoll[i] = packet.ReadShort();
            }

            var guildRankNum = packet.ReadChar();
            var jailMap = packet.ReadShort();

            //unused by eoserv - contains flood rates for commands, etc.
            packet.Seek(12, SeekOrigin.Current);

            var firstTimePlayer = packet.ReadChar() == 2; //signal that the player should see the "first timer" message

            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing terminating 255 byte", packet);

            return new LoginRequestGrantedData()
                .WithPlayerID(playerID)
                .WithCharacterID(characterID)
                .WithMapID(mapID)
                .WithMapRID(mapRid)
                .WithMapLen(mapLen)
                .WithEifRID(eifRid)
                .WithEifLen(eifLen)
                .WithEnfRID(enfRid)
                .WithEnfLen(enfLen)
                .WithEsfRID(esfRid)
                .WithEsfLen(esfLen)
                .WithEcfRID(ecfRid)
                .WithEcfLen(ecfLen)
                .WithName(characterName)
                .WithTitle(characterTitle)
                .WithGuildName(guildName)
                .WithGuildRank(guildRank)
                .WithClassId(classID)
                .WithGuildTag(paddedGuildTag)
                .WithAdminLevel(adminLevel)
                .WithCharacterStats(characterStats)
                .WithPaperdoll(paperDoll)
                .WithGuildRankNum(guildRankNum)
                .WithJailMap(jailMap)
                .WithFirstTimePlayer(firstTimePlayer);
        }
    }
}
