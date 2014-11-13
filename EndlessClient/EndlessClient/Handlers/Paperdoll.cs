using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Paperdoll
	{
		//subLoc == 0 for slot1, != 0 for slot2
		public static void EquipItem(short id, byte subLoc = 0)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized) return;

			Packet pkt = new Packet(PacketFamily.PaperDoll, PacketAction.Add);
			pkt.AddShort(id);
			pkt.AddChar(subLoc);

			client.SendPacket(pkt);
		}

		public static void PaperdollAgree(Packet pkt)
		{
			Avatar.AvatarAgree(pkt); //same logic in the beginning of the packet

			short itemID = pkt.GetShort();
			int characterAmount = pkt.GetThree();
			byte subLoc = pkt.GetByte();
			CharStatData data = new CharStatData
			{
				maxhp = pkt.GetShort(),
				maxtp = pkt.GetShort(),
				disp_str = pkt.GetShort(),
				disp_int = pkt.GetShort(),
				disp_wis = pkt.GetShort(),
				disp_agi = pkt.GetShort(),
				disp_con = pkt.GetShort(),
				disp_cha = pkt.GetShort(),
				mindam = pkt.GetShort(),
				maxdam = pkt.GetShort(),
				accuracy = pkt.GetShort(),
				evade = pkt.GetShort(),
				armor = pkt.GetShort()
			};
			
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(itemID, characterAmount);
		}
	}
}
