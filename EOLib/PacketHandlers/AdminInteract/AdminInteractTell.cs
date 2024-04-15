using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Response to $info <character> command.
    /// </summary>
    [AutoMappedType]
    public class AdminInteractTell: InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.AdminInteract;

        public override PacketAction Action => PacketAction.Tell;

        public AdminInteractTell(IPlayerInfoProvider playerInfoProvider,
                                 IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var name = packet.ReadBreakString();

            var stats = new Dictionary<CharacterStat, int>();
            stats[CharacterStat.Usage] = packet.ReadInt();
            if (packet.ReadByte() != 255 || packet.ReadByte() != 255)
                return false;

            stats[CharacterStat.Experience] = packet.ReadInt();
            stats[CharacterStat.Level] = packet.ReadChar();

            var mapId = packet.ReadShort();
            var mapCoords = new MapCoordinate(packet.ReadShort(), packet.ReadShort());

            stats[CharacterStat.HP] = packet.ReadShort();
            stats[CharacterStat.MaxHP] = packet.ReadShort();
            stats[CharacterStat.TP] = packet.ReadShort();
            stats[CharacterStat.MaxTP] = packet.ReadShort();

            stats[CharacterStat.Strength] = packet.ReadShort();
            stats[CharacterStat.Intelligence] = packet.ReadShort();
            stats[CharacterStat.Wisdom] = packet.ReadShort();
            stats[CharacterStat.Agility] = packet.ReadShort();
            stats[CharacterStat.Constitution] = packet.ReadShort();
            stats[CharacterStat.Charisma] = packet.ReadShort();

            stats[CharacterStat.MaxDam] = packet.ReadShort();
            stats[CharacterStat.MinDam] = packet.ReadShort();
            stats[CharacterStat.Accuracy] = packet.ReadShort();
            stats[CharacterStat.Evade] = packet.ReadShort();
            stats[CharacterStat.Armor] = packet.ReadShort();

            stats[CharacterStat.Light] = packet.ReadShort();
            stats[CharacterStat.Dark] = packet.ReadShort();
            stats[CharacterStat.Fire] = packet.ReadShort();
            stats[CharacterStat.Water] = packet.ReadShort();
            stats[CharacterStat.Earth] = packet.ReadShort();
            stats[CharacterStat.Wind] = packet.ReadShort();

            stats[CharacterStat.Weight] = packet.ReadChar();
            stats[CharacterStat.MaxWeight] = packet.ReadChar();

            foreach (var notifier in _userInterfaceNotifiers)
            {
                notifier.NotifyCharacterInfo(name, mapId, mapCoords, new CharacterStats(stats));
            }

            return true;
        }
    }
}
