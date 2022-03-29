using System;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void PlayerItemDropEvent(int characterAmount, byte weight, byte maxWeight, OldMapItem item);
    public delegate void RemoveMapItemEvent(short itemUID);
    public delegate void JunkItemEvent(short itemID, int numJunked, int numRemaining, byte weight, byte maxWeight);
    public delegate void ItemChangeEvent(bool newItemObtained, short id, int amount, byte weight);

    partial class PacketAPI
    {
        public event ItemChangeEvent OnItemChange;

        private void _createItemMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Obtain), _handleItemObtain, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Kick), _handleItemKick, true);
            //todo: handle ITEM_ACCEPT (ExpReward item type) (I think it shows the level up animation?)
        }

        private void _handleItemObtain(OldPacket pkt)
        {
            if (OnItemChange == null) return;

            short id = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();

            OnItemChange(true, id, amount, weight);
        }

        private void _handleItemKick(OldPacket pkt)
        {
            if (OnItemChange == null) return;

            short id = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();

            OnItemChange(false, id, amount, weight);
        }
    }
}
