
using EOLib;
using EOLib.Data;

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

		public static void UnequipItem(short id, byte subLoc = 0)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized) return;

			Packet pkt = new Packet(PacketFamily.PaperDoll, PacketAction.Remove);
			pkt.AddShort(id);
			pkt.AddChar(subLoc);

			client.SendPacket(pkt);
		}

		public static void RequestPaperdoll(short charId)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized) return;

			Packet pkt = new Packet(PacketFamily.PaperDoll, PacketAction.Request);
			pkt.AddShort(charId);

			client.SendPacket(pkt);
		}

		public static void PaperdollAgree(Packet pkt) //this is only ever sent to MainPlayer (avatar handles other players)
		{
			Avatar.AvatarAgree(pkt); //same logic in the beginning of the packet

			short itemId = pkt.GetShort();
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
			}; //todo: apply the updated stats data to the character

			EndlessClient.Character c;
			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(itemId);
			(c = World.Instance.MainPlayer.ActiveCharacter).UpdateInventoryItem(itemId, characterAmount);
			c.PaperDoll[(int)rec.GetEquipLocation() + subLoc] = 0;
			EOGame.Instance.Hud.ShowEquippedInPaperdollDialog(rec, rec.GetEquipLocation());
		}

		public static void PaperdollRemove(Packet pkt) //this is only ever sent to MainPlayer (avatar handles other players)
		{
			if (pkt.GetShort() != World.Instance.MainPlayer.ActiveCharacter.ID)
				return;

			if ((AvatarSlot) pkt.GetChar() != AvatarSlot.Clothes)
				return;

			pkt.GetChar(); //sound : 0

			//the $strip command does this wrong...somehow the original client is smart enough to figure it out
			short boots = pkt.GetShort();
			if(pkt.Length != 45) pkt.Skip(sizeof(short) * 3); //three zeroes
			short armor = pkt.GetShort();
			if(pkt.Length != 45) pkt.Skip(sizeof (short)); // one zero
			short hat = pkt.GetShort();
			short shield, weapon;
			if (pkt.Length != 45)
			{
				shield = pkt.GetShort();
				weapon = pkt.GetShort();
			}
			else
			{
				weapon = pkt.GetShort();
				shield = pkt.GetShort();
			}

			short itemId = pkt.GetShort();
			byte subLoc = pkt.GetChar();

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
			}; //todo: apply the updated stats data to the character

			EndlessClient.Character c = World.Instance.MainPlayer.ActiveCharacter;
			c.RenderData.SetBoots(boots);
			c.RenderData.SetHat(hat);
			c.RenderData.SetShield(shield);
			c.RenderData.SetWeapon(weapon);
			c.RenderData.SetArmor(armor);
			c.PaperDoll[(int)World.Instance.EIF.GetItemRecordByID(itemId).GetEquipLocation() + subLoc] = 0;
			c.UpdateInventoryItem(itemId, 1, true); //add to existing quantity
		}

		public static void PaperdollReply(Packet pkt) //sent when showing a paperdoll for a character
		{
			pkt.GetBreakString(); //name

			//need to be applied to the character that is passed to the dialog
			string home = pkt.GetBreakString();
			string partner = pkt.GetBreakString();
			string title = pkt.GetBreakString();
			string guild = pkt.GetBreakString();
			string rank = pkt.GetBreakString();

			short playerID = pkt.GetShort();
			byte clas = pkt.GetChar();
			byte gender = pkt.GetChar();

			if (pkt.GetChar() != 0) return;

			short[] paperdoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
			for (int i = 0; i < (int) EquipLocation.PAPERDOLL_MAX; ++i)
				paperdoll[i] = pkt.GetShort();

			/*Party Icon type (group or not) = */pkt.GetChar();
			
			EndlessClient.Character c;
			if (World.Instance.MainPlayer.ActiveCharacter.ID == playerID)
			{
				//paperdoll requested for main player, all info should be up to date
				c = World.Instance.MainPlayer.ActiveCharacter;
				c.PaperDoll = paperdoll; //make sure this is up-to-date for MainPlayer as well
			}
			else
			{
				if ((c = World.Instance.ActiveMapRenderer.GetOtherPlayer(playerID)) != null)
				{
					c.Class = clas;
					c.RenderData.SetGender(gender);
					c.Title = title;
					c.GuildName = guild;
					c.PaperDoll = paperdoll;
				}
			}

			if (c != null)
			{
				EOPaperdollDialog dlg = new EOPaperdollDialog(EOGame.Instance, c, home, partner, guild, rank);
				dlg.DialogClosing += (sender, args) => EOGame.Instance.Hud.ClearPaperdollDialog();
				EOGame.Instance.Hud.SetPaperdollDialog(dlg);
			}
		}
	}
}
