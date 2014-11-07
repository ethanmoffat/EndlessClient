using EOLib;

namespace EndlessClient.Handlers
{
	public static class Item
	{
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
		/// Item is added to the map
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
	}
}
