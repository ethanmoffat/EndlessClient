using System;
using System.Collections.Generic;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Chest
	{
		private static byte lastX, lastY;

		public static bool ChestOpen(byte x, byte y)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			lastX = x;
			lastY = y;

			Packet toSend = new Packet(PacketFamily.Chest, PacketAction.Open);
			toSend.AddChar(x);
			toSend.AddChar(y);

			return client.SendPacket(toSend);
		}

		public static void ChestOpenResponse(Packet pkt)
		{
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();

			if(x != lastX || y != lastY)
				EODialog.Show("Uh oh, this is a different chest than you just opened.");

			List<Tuple<short, int>> chestItems = new List<Tuple<short, int>>();
			int numRemaining = pkt.PeekEndString().Length/5;
			for (int i = 0; i < numRemaining; ++i)
			{
				chestItems.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}

			MapChest update = World.Instance.ActiveMapRenderer.MapRef.Chests.Find(_c => _c.x == x && _c.y == y);
			if (update != null)
			{
				update.backoff = false;
			}

			EOChestDialog chestDialog = new EOChestDialog(x, y, chestItems);
			lastX = lastY = 255;
		}

		public static bool ChestTake(byte x, byte y, short itemID)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Chest, PacketAction.Take);
			pkt.AddChar(x);
			pkt.AddChar(y);
			pkt.AddShort(itemID);

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
		/// </summary>
		public static void ChestGetResponse(Packet pkt)
		{
			if (EOChestDialog.Instance == null) return;

			short takenID = pkt.GetShort();
			int takenAmount = pkt.GetThree();
			byte characterWeight = pkt.GetChar();
			byte characterMaxWeight = pkt.GetChar();

			int numLeft = pkt.GetEndString().Length / 5;
			List<Tuple<short, int>> newItems = new List<Tuple<short, int>>(numLeft);
			for (int i = 0; i < numLeft; ++i)
			{
				newItems.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}

			//update chest dialog
			EOChestDialog.Instance.UpdateChestItemAmount(takenID, takenAmount);
			EOChestDialog.Instance.InitializeItems(newItems);

			//update item inventory
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(takenID, takenAmount, characterWeight, characterMaxWeight, true);
			//update stats
			EOGame.Instance.Hud.RefreshStats();
		}

		/// <summary>
		/// Handler for CHEST_AGREE packet, sent as update to other characters near a chest that was modified by another player
		/// </summary>
		public static void ChestAgreeResponse(Packet pkt)
		{
			//no need to do anything with the packet if the chest isn't open
			if (EOChestDialog.Instance == null) return;

			int numLeft = pkt.GetEndString().Length / 5;
			List<Tuple<short, int>> newItems = new List<Tuple<short, int>>(numLeft);
			for (int i = 0; i < numLeft; ++i)
			{
				newItems.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}
			EOChestDialog.Instance.InitializeItems(newItems);
		}
	}
}
