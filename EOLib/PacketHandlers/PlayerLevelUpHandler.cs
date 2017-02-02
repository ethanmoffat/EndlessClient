// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;

namespace EOLib.PacketHandlers
{
    public class PlayerLevelUpHandler : NPCLeaveMapHandler
    {
        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Accept;

        public PlayerLevelUpHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    ICharacterRepository characterRepository,
                                    IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                    IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers) { }

        public override bool HandlePacket(IPacket packet)
        {
            if (!base.HandlePacket(packet))
                return false;

            var level = packet.ReadChar();
            var stat  = packet.ReadShort();
            var skill = packet.ReadShort();
            var maxhp = packet.ReadShort();
            var maxtp = packet.ReadShort();
            var maxsp = packet.ReadShort();

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Level, level)
                .WithNewStat(CharacterStat.StatPoints, stat)
                .WithNewStat(CharacterStat.SkillPoints, skill)
                .WithNewStat(CharacterStat.MaxHP, maxhp)
                .WithNewStat(CharacterStat.MaxTP, maxtp)
                .WithNewStat(CharacterStat.MaxSP, maxsp);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            //todo: show emote once emotes are supported
            //    OldWorld.Instance.MainPlayer.ActiveCharacter.Emote(Emote.LevelUp);
            //    OldWorld.Instance.ActiveCharacterRenderer.PlayerEmote();

            return true;
        }
    }

    public class PlayerLevelUpFromSpellCastHandler : PlayerLevelUpHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public override PacketAction Action => PacketAction.Accept;

        public PlayerLevelUpFromSpellCastHandler(IPlayerInfoProvider playerInfoProvider,
                                                 ICurrentMapStateRepository currentMapStateRepository,
                                                 ICharacterRepository characterRepository,
                                                 IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                                 IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers) { }
    }
}
