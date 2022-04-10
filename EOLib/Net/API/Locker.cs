using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public delegate void LockerUpgradeEvent(int goldRemaining, byte lockerUpgrades);

        public event LockerUpgradeEvent OnLockerUpgrade;

        private void _createLockerMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Locker, PacketAction.Buy), _handleLockerBuy, true);
        }

        /// <summary>
        /// Handles LOCKER_BUY from server when buying a locker unit upgrade
        /// </summary>
        /// <param name="pkt"></param>
        private void _handleLockerBuy(OldPacket pkt)
        {
            if (OnLockerUpgrade != null)
                OnLockerUpgrade(pkt.GetInt(), pkt.GetChar()); //gold remaining, num upgrades
        }
    }
}
