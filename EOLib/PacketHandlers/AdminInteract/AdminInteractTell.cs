using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Response to $info <character> command.
    /// </summary>
    [AutoMappedType]
    public class AdminInteractTell: InGameOnlyPacketHandler<AdminInteractTellServerPacket>
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

        public override bool HandlePacket(AdminInteractTellServerPacket packet)
        {
            var stats = new Dictionary<CharacterStat, int>
            {
                [CharacterStat.Usage] = packet.Usage,

                [CharacterStat.Experience] = packet.Exp,
                [CharacterStat.Level] = packet.Level,

                [CharacterStat.HP] = packet.Stats.Hp,
                [CharacterStat.MaxHP] = packet.Stats.MaxHp,
                [CharacterStat.TP] = packet.Stats.Tp,
                [CharacterStat.MaxTP] = packet.Stats.MaxTp,

                [CharacterStat.Strength] = packet.Stats.BaseStats.Str,
                [CharacterStat.Intelligence] = packet.Stats.BaseStats.Intl,
                [CharacterStat.Wisdom] = packet.Stats.BaseStats.Wis,
                [CharacterStat.Agility] = packet.Stats.BaseStats.Agi,
                [CharacterStat.Constitution] = packet.Stats.BaseStats.Con,
                [CharacterStat.Charisma] = packet.Stats.BaseStats.Cha,

                [CharacterStat.MaxDam] = packet.Stats.SecondaryStats.MaxDamage,
                [CharacterStat.MinDam] = packet.Stats.SecondaryStats.MinDamage,
                [CharacterStat.Accuracy] = packet.Stats.SecondaryStats.Accuracy,
                [CharacterStat.Evade] = packet.Stats.SecondaryStats.Evade,
                [CharacterStat.Armor] = packet.Stats.SecondaryStats.Armor,

                [CharacterStat.Light] = packet.Stats.ElementalStats.Light,
                [CharacterStat.Dark] = packet.Stats.ElementalStats.Dark,
                [CharacterStat.Fire] = packet.Stats.ElementalStats.Fire,
                [CharacterStat.Water] = packet.Stats.ElementalStats.Water,
                [CharacterStat.Earth] = packet.Stats.ElementalStats.Earth,
                [CharacterStat.Wind] = packet.Stats.ElementalStats.Wind,

                [CharacterStat.Weight] = packet.Weight.Current,
                [CharacterStat.MaxWeight] = packet.Weight.Max
            };

            foreach (var notifier in _userInterfaceNotifiers)
            {
                notifier.NotifyCharacterInfo(packet.Name, packet.MapId, new MapCoordinate(packet.MapCoords), new CharacterStats(stats));
            }

            return true;
        }
    }
}
