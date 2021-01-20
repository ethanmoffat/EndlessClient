using System;
using System.Collections.Generic;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void EffectPotionUseEvent(short playerID, int effectID);

    public struct TimedMapHPDrainData
    {
        private readonly short _playerID;
        private readonly byte _playerPercentHealth;
        private readonly short _damageDealt;

        public short PlayerID => _playerID;
        public byte PlayerPercentHealth => _playerPercentHealth;
        public short DamageDealt => _damageDealt;

        internal TimedMapHPDrainData(short playerID, byte percentHealth, short damageDealt)
        {
            _playerID = playerID;
            _playerPercentHealth = percentHealth;
            _damageDealt = damageDealt;
        }
    }

    partial class PacketAPI
    {
        public event EffectPotionUseEvent OnEffectPotion;

        private void _createEffectMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Effect, PacketAction.Player), _handleEffectPlayer, true);
        }

        //potion effect (only known use based on eoserv code)
        private void _handleEffectPlayer(OldPacket pkt)
        {
            if (OnEffectPotion != null)
                OnEffectPotion(playerID: pkt.GetShort(),
                               effectID: pkt.GetThree());
        }
    }
}
