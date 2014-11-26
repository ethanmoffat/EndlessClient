using EOLib;

namespace EndlessClient.Handlers
{
	public static class Item
	{
		public static bool GetItem(short uid)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Get);
			pkt.AddShort(uid);

			return client.SendPacket(pkt);
		}

		public static void DropItem(short id, int amount, byte x = 255, byte y = 255) //255 means use character's current location
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Drop);
			pkt.AddShort(id);
			pkt.AddInt(amount);
			if (x == 255 && y == 255)
			{
				pkt.AddByte(x);
				pkt.AddByte(y);
			}
			else
			{
				pkt.AddChar(x);
				pkt.AddChar(y);
			}

			client.SendPacket(pkt);
		}

		public static void JunkItem(short id, int amount)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return;
			
			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Junk);
			pkt.AddShort(id);
			pkt.AddInt(amount);

			client.SendPacket(pkt);
		}

		/// <summary>
		/// Sent when an Item is dropped by the MainPlayer
		/// See ItemAddResponse for when another player drops an item
		/// </summary>
		public static void ItemDropResponse(Packet pkt)
		{
			short _id = pkt.GetShort();
			int _amount = pkt.GetThree();
			int characterAmount = pkt.GetInt(); //amount remaining for the character
			MapItem item = new MapItem
			{
				id = _id,
				amount = _amount,
				uid = pkt.GetShort(),
				x = pkt.GetChar(),
				y = pkt.GetChar()
			};
			byte characterWeight = pkt.GetChar(), characterMaxWeight = pkt.GetChar(); //character adjusted weights

			World.Instance.ActiveMapRenderer.MapItems.Add(item);
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(_id, characterAmount, characterWeight, characterMaxWeight);
		}

		/// <summary>
		/// Item is added to the map (sent when another player drops an item)
		/// </summary>
		public static void ItemAddResponse(Packet pkt)
		{
			World.Instance.ActiveMapRenderer.MapItems.Add(new MapItem { id = pkt.GetShort(), uid = pkt.GetShort(), amount = pkt.GetThree(), x = pkt.GetChar(), y = pkt.GetChar() });
		}

		public static void ItemRemoveResponse(Packet pkt)
		{
			short itemUid = pkt.GetShort();
			MapItem toRemove;
			if ((toRemove = World.Instance.ActiveMapRenderer.MapItems.Find(mi => mi.uid == itemUid)).uid == itemUid)
			{
				World.Instance.ActiveMapRenderer.MapItems.Remove(toRemove);
			}
		}

		public static void ItemJunkResponse(Packet pkt)
		{
			short id = pkt.GetShort();
			/*int amountRemoved = */pkt.GetThree();//don't really care - just math it
			int amountRemaining = pkt.GetInt();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountRemaining, weight, maxWeight);
		}

		public static void ItemGetResponse(Packet pkt)
		{
			short uid = pkt.GetShort();
			short id = pkt.GetShort();
			int amountTaken = pkt.GetThree();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			if (uid != 0)
			{
				MapItem toRemove;
				if ((toRemove = World.Instance.ActiveMapRenderer.MapItems.Find(mi => mi.uid == uid)).uid == uid)
				{
					World.Instance.ActiveMapRenderer.MapItems.Remove(toRemove);
					toRemove = new MapItem
					{
						amount = toRemove.amount - amountTaken,
						uid = uid,
						id = id,
						x = toRemove.x,
						y = toRemove.y
					};
					if (toRemove.amount > 0) World.Instance.ActiveMapRenderer.MapItems.Add(toRemove);
				}
			}

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountTaken, weight, maxWeight, true);//true: adding amounts if item ID exists
			EOGame.Instance.Hud.SetStatusLabel(string.Format("[ Info ] You picked up {0} {1}", amountTaken, World.Instance.EIF.GetItemRecordByID(id).Name));
		}
	}
}
