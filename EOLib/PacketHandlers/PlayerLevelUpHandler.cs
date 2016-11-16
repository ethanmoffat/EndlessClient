// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Net;

namespace EOLib.PacketHandlers
{
    public class PlayerLevelUpHandler : NPCLeaveMapHandler
    {
        public override PacketFamily Family { get { return PacketFamily.NPC; } }

        public override PacketAction Action { get { return PacketAction.Accept; } }

        public PlayerLevelUpHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    ICharacterRepository characterRepository,
                                    IEnumerable<INPCAnimationNotifier> npcAnimationNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, npcAnimationNotifiers) { }

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

            return true;
        }
    }

    public class PlayerLevelUpFromSpellCastHandler : PlayerLevelUpHandler
    {
        public override PacketFamily Family { get { return PacketFamily.Cast; } }

        public override PacketAction Action { get { return PacketAction.Accept; } }

        public PlayerLevelUpFromSpellCastHandler(IPlayerInfoProvider playerInfoProvider,
                                                 ICurrentMapStateRepository currentMapStateRepository,
                                                 ICharacterRepository characterRepository,
                                                 IEnumerable<INPCAnimationNotifier> npcAnimationNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, npcAnimationNotifiers) { }
    }
}
